using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using MoviesApi.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    public class RatingController : Controller
    {
        private readonly RatingContext _ratingContext;
        private readonly MovieContext _movieContext;
        private readonly UserContext _userContext;


        public RatingController(RatingContext ratingContext, MovieContext movieContext, UserContext userContext)
        {
            _ratingContext = ratingContext;
            _movieContext = movieContext;
            _userContext = userContext;

            if (!_ratingContext.Ratings.Any())
            {
                _ratingContext.Ratings.Add(new Rating { MovieId = 1, Score = 3, UserId = 1});
                _ratingContext.Ratings.Add(new Rating { MovieId = 2, Score = 2, UserId = 2 });
                _ratingContext.Ratings.Add(new Rating { MovieId = 3, Score = 1, UserId = 1 });
                _ratingContext.Ratings.Add(new Rating { MovieId = 4, Score = 4, UserId = 2 });
                _ratingContext.SaveChanges();
            }
        }

        [HttpGet]
        public IEnumerable<Rating> GetAll()
        {
            return _ratingContext.Ratings.ToList();
        }

        [HttpGet("{id}", Name = "GetRating")]
        public IActionResult GetById(int id)
        {
            var item = _ratingContext.Ratings.FirstOrDefault(t => t.MovieId == id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }


        // API Specification Part D
        // AddOrUpdateUserRating
        [HttpPost]
        public HttpStatusCode AddOrUpdateUserRating([FromBody] Rating item)
        {
            if (item == null || item.Score < 1 || item.Score > 5)
            {
                return HttpStatusCode.BadRequest;
            }

            if (!_movieContext.Movies.Any(m => m.Id == item.Id) || !_userContext.Users.Any(u => u.Id == item.Id))
            {
                return HttpStatusCode.NotFound;
            }

            var usersRatingForThisMovie = _ratingContext.Ratings.Where(m => m.MovieId == item.MovieId).Where(u => u.UserId == item.UserId);

            if (!usersRatingForThisMovie.Any())
            {
                _ratingContext.Ratings.Add(item);
                _ratingContext.SaveChanges();
            }
            else
            {
                var oldRating = usersRatingForThisMovie.FirstOrDefault();
                _ratingContext.Remove(oldRating);
                _ratingContext.Ratings.Add(item);
                _ratingContext.SaveChanges();
            }

            return HttpStatusCode.OK;
        }
    }
}
