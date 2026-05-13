using CommentService.Clients;
using CommentService.DTOs;
using CommentService.Entities;
using CommentService.Interfaces;

namespace CommentService.Services {
    public class CommentService : ICommentService {
        private readonly ICommentRepository _commentRepository;
        private readonly PostServiceClient _postServiceClient;
        private readonly AuthServiceClient _authClient;

        public CommentService(ICommentRepository commentRepository, PostServiceClient postServiceClient, AuthServiceClient authClient) {
            _commentRepository = commentRepository;
            _postServiceClient = postServiceClient;
            _authClient = authClient;
        }

        public async Task<CommentResponseDto> AddComment(int userId, CreateCommentDto dto) {
            // Normalize ParentCommentId: 0 or non-positive means it's a top-level comment
            if (dto.ParentCommentId.HasValue && dto.ParentCommentId.Value <= 0) {
                dto.ParentCommentId = null;
            }

            // Validate parent comment exists if replying
            if (dto.ParentCommentId.HasValue) {
                Comment? parent = await _commentRepository.GetCommentById(dto.ParentCommentId.Value);
                if (parent == null || parent.IsDeleted) {
                    throw new InvalidOperationException("Parent comment not found or has been deleted.");
                }
            }

            Comment comment = new Comment {
                PostId = dto.PostId,
                UserId = userId,
                Content = dto.Content,
                ParentCommentId = dto.ParentCommentId
            };

            Comment saved = await _commentRepository.AddComment(comment);

            // Only increment post's CommentCount for top-level comments
            if (!dto.ParentCommentId.HasValue) {
                await _postServiceClient.IncrementCommentCount(dto.PostId);
            }

            var response = MapToDto(saved);
            await EnrichComments(new List<CommentResponseDto> { response });
            return response;
        }

        public async Task<List<CommentResponseDto>> GetCommentsByPost(int postId) {
            List<Comment> allComments = await _commentRepository.GetCommentsByPost(postId);
            var tree = BuildTree(allComments, null);
            await EnrichComments(Flatten(tree));
            return tree;
        }

        private async Task EnrichComments(List<CommentResponseDto> comments) {
            if (comments == null || !comments.Any()) return;

            var userIds = comments.Select(c => c.UserId).Distinct().ToList();
            var users = await _authClient.GetUsersByIds(userIds);
            var userDict = users.ToDictionary(u => u.UserId);

            foreach (var comment in comments) {
                if (userDict.TryGetValue(comment.UserId, out var user)) {
                    comment.UserName = user.UserName;
                    comment.FullName = user.FullName;
                    comment.AvatarUrl = user.AvatarUrl;
                }
            }
        }

        private List<CommentResponseDto> Flatten(List<CommentResponseDto> tree) {
            var list = new List<CommentResponseDto>();
            foreach (var node in tree) {
                list.Add(node);
                if (node.Replies.Any()) {
                    list.AddRange(Flatten(node.Replies));
                }
            }
            return list;
        }

        public async Task<List<CommentedPostDto>> GetPostsCommentedByUser(int userId) {
            List<Comment> userComments = await _commentRepository.GetCommentsByUser(userId);

            // Group by PostId and aggregate stats
            return userComments
                .GroupBy(c => c.PostId)
                .Select(g => new CommentedPostDto {
                    PostId = g.Key,
                    FirstCommentedAt = g.Min(c => c.CreatedAt),
                    LastCommentedAt = g.Max(c => c.CreatedAt),
                    CommentCount = g.Count()
                })
                .OrderByDescending(p => p.LastCommentedAt)
                .ToList();
        }

        public async Task<bool> DeleteComment(int commentId, int userId) {
            // Check if it's a top-level comment before deleting (to decrement post count)
            Comment? comment = await _commentRepository.GetCommentById(commentId);
            if (comment == null || comment.UserId != userId || comment.IsDeleted) return false;

            bool isTopLevel = !comment.ParentCommentId.HasValue;
            int postId = comment.PostId;

            bool deleted = await _commentRepository.SoftDeleteComment(commentId, userId);

            if (deleted && isTopLevel) {
                await _postServiceClient.DecrementCommentCount(postId);
            }

            return deleted;
        }

        // Recursively builds a nested comment tree from a flat list
        private List<CommentResponseDto> BuildTree(List<Comment> allComments, int? parentId) {
            return allComments
                .Where(c => c.ParentCommentId == parentId)
                .Select(c => {
                    CommentResponseDto dto = MapToDto(c);
                    dto.Replies = BuildTree(allComments, c.CommentId);
                    return dto;
                })
                .ToList();
        }

        private CommentResponseDto MapToDto(Comment comment) {
            return new CommentResponseDto {
                CommentId = comment.CommentId,
                PostId = comment.PostId,
                UserId = comment.UserId,
                UserName = null, // Populated by EnrichComments
                FullName = null, // Populated by EnrichComments
                AvatarUrl = null, // Populated by EnrichComments
                Content = comment.Content,
                ParentCommentId = comment.ParentCommentId,
                IsDeleted = comment.IsDeleted,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                Replies = new List<CommentResponseDto>()
            };
        }
    }
}
