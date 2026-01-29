namespace Affiliance_core.ApiHelper
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Data { get; }

        public int Page {  get;  }
        public int PageSize {  get; }
        public int TotalCount { get; }

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public PagedResult(IEnumerable<T> data , int page , int pagesize , int totalCount )
        {
            Data = data;
            Page = page;
            PageSize = pagesize;
           TotalCount = totalCount;
            
        }
    }
}
