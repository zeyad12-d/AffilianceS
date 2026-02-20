using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.ReviewDto;
using Affiliance_core.Entites;
using Affiliance_core.interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Affiliance_Applaction.services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PagedResult<ReviewDto>>> GetReviewsAsync(int reviewedId, ReviewFilterDto? filter = null)
        {
            filter ??= new ReviewFilterDto();
            var query = _unitOfWork.Repository<Review>()
                .GetQueryable()
                .Where(r => r.ReviewedId == reviewedId);

            if (filter.Rating.HasValue)
                query = query.Where(r => r.Rating == filter.Rating.Value);

            if (filter.CampaignId.HasValue)
                query = query.Where(r => r.CampaignId == filter.CampaignId.Value);

            var total = await query.CountAsync();
            var reviews = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Include(r => r.Reviewer)
                .Include(r => r.Campaign)
                .ToListAsync();

            var reviewsDto = _mapper.Map<List<ReviewDto>>(reviews);
            var pagedResult = new PagedResult<ReviewDto>(reviewsDto, filter.Page, filter.PageSize, total);
            return ApiResponse<PagedResult<ReviewDto>>.CreateSuccess(pagedResult, "Reviews retrieved successfully");
        }

        public async Task<ApiResponse<PagedResult<ReviewDto>>> GetReviewsGivenAsync(int reviewerId, int page = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Repository<Review>()
                .GetQueryable()
                .Where(r => r.ReviewerId == reviewerId);

            var total = await query.CountAsync();
            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(r => r.Reviewed)
                .Include(r => r.Campaign)
                .ToListAsync();

            var reviewsDto = _mapper.Map<List<ReviewDto>>(reviews);
            var pagedResult = new PagedResult<ReviewDto>(reviewsDto, page, pageSize, total);
            return ApiResponse<PagedResult<ReviewDto>>.CreateSuccess(pagedResult, "Reviews retrieved successfully");
        }

        public async Task<ApiResponse<AverageRatingDto>> GetAverageRatingAsync(int reviewedId)
        {
            var reviews = await _unitOfWork.Repository<Review>()
                .FindAsync(r => r.ReviewedId == reviewedId);

            var ratingDistribution = new Dictionary<byte, int>();
            for (byte i = 1; i <= 5; i++)
                ratingDistribution[i] = reviews.Count(r => r.Rating == i);

            var reviewsList = reviews.ToList();
            var averageRating = reviewsList.Count > 0 ? reviewsList.Average(r => (decimal)r.Rating) : 0;

            var result = new AverageRatingDto
            {
                AverageRating = (decimal)averageRating,
                TotalReviews = reviewsList.Count,
                RatingDistribution = ratingDistribution
            };

            return ApiResponse<AverageRatingDto>.CreateSuccess(result, "Average rating retrieved successfully");
        }

        public async Task<ApiResponse<ReviewDto>> CreateReviewAsync(int reviewerId, int reviewedId, int? campaignId, byte rating, string? comment)
        {
            if (rating < 1 || rating > 5)
                return ApiResponse<ReviewDto>.CreateFail("Rating must be between 1 and 5");

            var reviewer = await _unitOfWork.Repository<User>().GetByIdAsync(reviewerId);
            if (reviewer == null)
                return ApiResponse<ReviewDto>.CreateFail("Reviewer not found");

            var review = new Review
            {
                ReviewerId = reviewerId,
                ReviewedId = reviewedId,
                CampaignId = campaignId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Review>().AddAsync(review);
            await _unitOfWork.CompleteAsync();

            var reviewDto = _mapper.Map<ReviewDto>(review);
            return ApiResponse<ReviewDto>.CreateSuccess(reviewDto, "Review created successfully");
        }

        public async Task<ApiResponse<ReviewDto>> UpdateReviewAsync(int reviewId, int reviewerId, UpdateReviewDto dto)
        {
            var review = await _unitOfWork.Repository<Review>().GetByIdAsync(reviewId);
            if (review == null)
                return ApiResponse<ReviewDto>.CreateFail("Review not found");

            if (review.ReviewerId != reviewerId)
                return ApiResponse<ReviewDto>.CreateFail("You can only update your own reviews");

            review.Rating = (byte)dto.Rating;
            review.Comment = dto.Comment;

            _unitOfWork.Repository<Review>().Update(review);
            await _unitOfWork.CompleteAsync();

            var reviewDto = _mapper.Map<ReviewDto>(review);
            return ApiResponse<ReviewDto>.CreateSuccess(reviewDto, "Review updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteReviewAsync(int reviewId, int reviewerId)
        {
            var review = await _unitOfWork.Repository<Review>().GetByIdAsync(reviewId);
            if (review == null)
                return ApiResponse<bool>.CreateFail("Review not found");

            if (review.ReviewerId != reviewerId)
                return ApiResponse<bool>.CreateFail("You can only delete your own reviews");

            _unitOfWork.Repository<Review>().Delete(review);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Review deleted successfully");
        }

        public async Task<ApiResponse<CompanyReviewSummaryDto>> GetCompanyReviewSummaryAsync(int companyId)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(companyId);
            if (company == null)
                return ApiResponse<CompanyReviewSummaryDto>.CreateFail("Company not found");

            var reviews = await _unitOfWork.Repository<Review>()
                .FindAsync(r => r.ReviewedId == company.UserId);

            var reviewsList = reviews.ToList();
            var averageRating = reviewsList.Count > 0 ? reviewsList.Average(r => (decimal)r.Rating) : 0;

            var recentReviews = reviewsList
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToList();

            var summary = new CompanyReviewSummaryDto
            {
                CompanyId = companyId,
                CompanyName = company.CampanyName,
                AverageRating = averageRating,
                TotalReviews = reviewsList.Count,
                FiveStarCount = reviewsList.Count(r => r.Rating == 5),
                FourStarCount = reviewsList.Count(r => r.Rating == 4),
                ThreeStarCount = reviewsList.Count(r => r.Rating == 3),
                TwoStarCount = reviewsList.Count(r => r.Rating == 2),
                OneStarCount = reviewsList.Count(r => r.Rating == 1),
                RecentReviews = _mapper.Map<List<ReviewDto>>(recentReviews)
            };

            return ApiResponse<CompanyReviewSummaryDto>.CreateSuccess(summary, "Company review summary retrieved successfully");
        }

        public async Task<ApiResponse<PagedResult<MarketerPublicReviewDto>>> GetMarketerPublicReviewsAsync(int marketerId, int page = 1, int pageSize = 10)
        {
            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<PagedResult<MarketerPublicReviewDto>>.CreateFail("Marketer not found");

            var query = _unitOfWork.Repository<Review>()
                .GetQueryable()
                .Where(r => r.ReviewedId == marketer.UserId);

            var total = await query.CountAsync();
            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(r => r.Reviewer)
                .Include(r => r.Campaign)
                .ToListAsync();

            var reviewsDto = reviews.Select(r => new MarketerPublicReviewDto
            {
                Id = r.Id,
                ReviewerName = $"{r.Reviewer?.FirstName} {r.Reviewer?.LastName}".Trim(),
                ReviewerAvatar = r.Reviewer?.ProfilePicture,
                Rating = r.Rating,
                Comment = r.Comment ?? string.Empty,
                CampaignTitle = r.Campaign?.Title,
                CreatedAt = r.CreatedAt
            }).ToList();

            var pagedResult = new PagedResult<MarketerPublicReviewDto>(reviewsDto, page, pageSize, total);
            return ApiResponse<PagedResult<MarketerPublicReviewDto>>.CreateSuccess(pagedResult, "Marketer reviews retrieved successfully");
        }
    }
}
