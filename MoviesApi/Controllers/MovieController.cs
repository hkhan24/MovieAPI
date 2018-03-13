using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesApi.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MoviesApi.Controllers
{
    [Route("api/[controller]/[action]")]
    public class MovieController : Controller
    {
        private readonly RatingContext _ratingContext;
        private readonly MovieContext _movieContext;
        private readonly UserContext _userContext;


        public MovieController(RatingContext ratingContext, MovieContext movieContext, UserContext userContext)
        {
            _movieContext = movieContext;
            _ratingContext = ratingContext;
            _userContext = userContext;

            if (_movieContext.Movies.Count() == 0)
            {
                _movieContext.Movies.Add(new Movie { Title = "Home Alone", Genre = Genre.Comedy, RunningTime = 123, YearOfRelease = 1990 });
                _movieContext.Movies.Add(new Movie { Title = "Die Hard", Genre = Genre.Action, RunningTime = 153, YearOfRelease = 1991 });
                _movieContext.Movies.Add(new Movie { Title = "Superman", Genre = Genre.SciFi, RunningTime = 223, YearOfRelease = 2006 });
                _movieContext.Movies.Add(new Movie { Title = "Batman", Genre = Genre.Any, RunningTime = 233, YearOfRelease = 2001 });
                _movieContext.SaveChanges();
            }
        }

        [HttpGet]
        public IEnumerable<Movie> GetAll()
        {
            return _movieContext.Movies.ToList();
        }

        [HttpPost]
        // API Part A
        // To Query please send the following request in postman:
        // {
        // "title":"batman"
        // "yearOfRelease": "2000"
        // "genre": "action"
        // }
        public ActionResult Query(HttpRequestMessage request, [FromBody] QueryCriteria query)
        {
            var movies = _movieContext.Movies.ToList();
            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Title))
                {
                    movies = movies.Where(m => m.Title.ToLower().Contains(query.Title.ToLower())).ToList();
                }
                if (query.YearOfRelease > 0)
                {
                    movies = movies.Where(m => m.YearOfRelease == query.YearOfRelease).ToList();
                }

                if (query.Genre != Genre.Any)
                {
                    movies = movies.Where(m => m.Genre == query.Genre).ToList();
                }

                if (movies.Any())
                {
                    return Ok(movies);
                }

                return NotFound();
            }

            return BadRequest();
        }

        // API Part B
        [HttpGet]
        public IEnumerable<Movie> GetTopFiveMoviesByAllTimeRating()
        {
            var ratings = _ratingContext.Ratings;
            var movies = _movieContext.Movies;

            foreach (var movie in movies)
            {
                var ratingsForMovie = ratings.Where(r => r.MovieId == movie.Id);
                var numberOfRatings = ratingsForMovie.Count();
                if (numberOfRatings > 0)
                {
                    var allRatings = ratingsForMovie.Select(r => r.Score);
                    var averageRating = allRatings.Sum(s => s) / numberOfRatings;
                    movie.AverageRating = averageRating;
                }
            }

            return movies.OrderBy(m => m.AverageRating).Take(5).ToList();
        }

        // API Part C
        [HttpGet("{id}")]
        public IActionResult GetByUsersRating(int id)
        {
            var ratings = _ratingContext.Ratings.Where(r => r.UserId == id).OrderByDescending(s => s.Score);

            var topFiveMovies = new List<Movie>();

            foreach (var rating in ratings)
            {
                topFiveMovies.Add(_movieContext.Movies.First(m => m.Id == rating.MovieId));
            }

            return new ObjectResult(topFiveMovies.Take(5));
        }
    }
}
