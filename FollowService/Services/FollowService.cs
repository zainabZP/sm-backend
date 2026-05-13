using FollowService.DTOs;
using FollowService.Entities;
using FollowService.Interfaces;

namespace FollowService.Services {
    public class FollowService : IFollowService {
        private readonly IFollowRepository _followRepository;
        private readonly AuthServiceClient _authServiceClient;

        public FollowService(IFollowRepository followRepository, AuthServiceClient authServiceClient) {
            _followRepository = followRepository;
            _authServiceClient = authServiceClient;
        }

        public async Task<FollowResponseDto?> FollowUser(int followerId, int followeeId) {
            if (followerId == followeeId) return null;

            Follow? existing = await _followRepository.GetFollow(followerId, followeeId);
            if (existing != null) return null;

            bool isPrivate = await _authServiceClient.IsUserPrivate(followeeId);

            Follow follow = new Follow {
                FollowerId = followerId,
                FolloweeId = followeeId,
                Status = isPrivate ? "PENDING" : "ACCEPTED"
            };

            Follow created = await _followRepository.CreateFollow(follow);

            if (!isPrivate) {
                await _authServiceClient.IncrementFollowerCount(followeeId);
                await _authServiceClient.IncrementFollowingCount(followerId);
            }

            return MapToDto(created);
        }

        public async Task<bool> UnfollowUser(int followerId, int followeeId) {
            Follow? follow = await _followRepository.GetFollow(followerId, followeeId);
            if (follow == null) return false;

            bool result = await _followRepository.DeleteFollow(followerId, followeeId);

            if (result && follow.Status == "ACCEPTED") {
                await _authServiceClient.DecrementFollowerCount(followeeId);
                await _authServiceClient.DecrementFollowingCount(followerId);
            }

            return result;
        }

        public async Task<bool> AcceptFollowRequest(int followId, int userId) {
            Follow? follow = await _followRepository.GetFollowById(followId);
            if (follow == null || follow.Status != "PENDING") return false;
            if (follow.FolloweeId != userId) return false;

            bool result = await _followRepository.UpdateFollowStatus(followId, "ACCEPTED");

            if (result) {
                await _authServiceClient.IncrementFollowerCount(follow.FolloweeId);
                await _authServiceClient.IncrementFollowingCount(follow.FollowerId);
            }

            return result;
        }

        public async Task<bool> RejectFollowRequest(int followId, int userId) {
            Follow? follow = await _followRepository.GetFollowById(followId);
            if (follow == null || follow.Status != "PENDING") return false;
            if (follow.FolloweeId != userId) return false;
            return await _followRepository.UpdateFollowStatus(followId, "REJECTED");
        }

        public async Task<List<FollowResponseDto>> GetFollowers(int userId) {
            List<Follow> followers = await _followRepository.GetFollowers(userId);
            var userIds = followers.Select(f => f.FollowerId).ToList();
            var users = await _authServiceClient.GetUsersByIds(userIds);
            var userDict = users.ToDictionary(u => u.UserId);

            var dtos = new List<FollowResponseDto>();
            foreach (var f in followers) {
                if (userDict.TryGetValue(f.FollowerId, out var user)) {
                    var dto = MapToDto(f);
                    dto.UserName = user.UserName;
                    dto.FullName = user.FullName;
                    dto.AvatarUrl = user.AvatarUrl;
                    dtos.Add(dto);
                } else {
                    // Ghost follower - cleanup
                    await _followRepository.DeleteFollow(f.FollowerId, f.FolloweeId);
                    await _authServiceClient.DecrementFollowerCount(f.FolloweeId);
                }
            }
            return dtos;
        }

        public async Task<List<FollowResponseDto>> GetFollowing(int userId) {
            List<Follow> following = await _followRepository.GetFollowing(userId);
            var userIds = following.Select(f => f.FolloweeId).ToList();
            var users = await _authServiceClient.GetUsersByIds(userIds);
            var userDict = users.ToDictionary(u => u.UserId);

            var dtos = new List<FollowResponseDto>();
            foreach (var f in following) {
                if (userDict.TryGetValue(f.FolloweeId, out var user)) {
                    var dto = MapToDto(f);
                    dto.UserName = user.UserName;
                    dto.FullName = user.FullName;
                    dto.AvatarUrl = user.AvatarUrl;
                    dtos.Add(dto);
                } else {
                    // Ghost followee - cleanup
                    await _followRepository.DeleteFollow(f.FollowerId, f.FolloweeId);
                    await _authServiceClient.DecrementFollowingCount(f.FollowerId);
                }
            }
            return dtos;
        }

        public async Task<List<FollowResponseDto>> GetPendingRequests(int userId) {
            List<Follow> pending = await _followRepository.GetPendingRequests(userId);
            var userIds = pending.Select(f => f.FollowerId).ToList();
            var users = await _authServiceClient.GetUsersByIds(userIds);
            var userDict = users.ToDictionary(u => u.UserId);

            var dtos = new List<FollowResponseDto>();
            foreach (var f in pending) {
                if (userDict.TryGetValue(f.FollowerId, out var user)) {
                    var dto = MapToDto(f);
                    dto.UserName = user.UserName;
                    dto.FullName = user.FullName;
                    dto.AvatarUrl = user.AvatarUrl;
                    dtos.Add(dto);
                }
            }
            return dtos;
        }

        public async Task<FollowResponseDto?> IsFollowing(int followerId, int followeeId) {
            Follow? follow = await _followRepository.GetFollow(followerId, followeeId);
            return follow != null ? MapToDto(follow) : null;
        }

        public async Task<bool> SyncUserCounts(int userId) {
            var followers = await _followRepository.GetFollowers(userId);
            var following = await _followRepository.GetFollowing(userId);
            await _authServiceClient.UpdateFollowCounts(userId, followers.Count, following.Count);
            return true;
        }

        public async Task<int> GetFollowerCount(int userId) {
            return await _followRepository.GetFollowerCount(userId);
        }

        public async Task<int> GetFollowingCount(int userId) {
            return await _followRepository.GetFollowingCount(userId);
        }

        private FollowResponseDto MapToDto(Follow follow) {
            return new FollowResponseDto {
                FollowId = follow.FollowId,
                FollowerId = follow.FollowerId,
                FolloweeId = follow.FolloweeId,
                Status = follow.Status,
                CreatedAt = follow.CreatedAt
            };
        }
    }
}