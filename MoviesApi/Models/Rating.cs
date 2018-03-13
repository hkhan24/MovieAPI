namespace MoviesApi.Models
{
    public class Rating
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public int MovieId { get; set; }
        public decimal Score { get; set; }
    }
}
