// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Samples.NotFlix;

/// <summary>
/// Static database of movies.
/// </summary>
public class MovieDatabase
{
    private List<Movie> RegisteredMovies = new();

    public MovieDatabase()
    {
        RegisteredMovies.AddRange(
            new Movie[]
            {
                new() {
                    Title = "Love Challenge",
                    Description = "Two competitive tennis players find themselves in a heated rivalry, not just on the court but also in their quest to win the heart of a fellow tennis star. As they battle through intense matches, they discover that love might be the ultimate prize.",
                    ReleaseYear = 2024,
                    Rating = "PG-13",
                    RunningTime = 102,
                    Genres = ["Romance"],
                    Starring = ["Alejandro Martinez", "Isabella Moore", "Mason Harris"],
                    Poster = "images/posters/lovechallenge.jpeg"
                },
                new()
                {
                    Title = "Rejuvenation",
                    Description = "A woman in her 50s, once the beloved host of a Pilates TV show, faces age discrimination and is fired. Desperate to reclaim her career, she takes a mysterious substance that transforms her into a younger version of herself, but at a horrifying cost.",
                    ReleaseYear = 2024,
                    Rating = "R",
                    RunningTime = 95,
                    Genres = ["Action"],
                    Starring = ["Sofia Rodriguez", "Harper Wright", "Emily Carter"],
                    Poster = "images/posters/rejuvenation.jpeg",
                },
                new()
                {
                    Title = "Roots of the Heart",
                    Description = "A man in his 30s embarks on a journey to Poland with his laid-back cousin to reconnect with his heritage. Along the way, they discover the true meaning of family and identity.",
                    ReleaseYear = 2019,
                    Rating = "PG-13",
                    RunningTime = 126,
                    Genres = ["Drama"],
                    Starring = ["Ethan Davis", "Liam Taylor"],
                    Poster = "images/posters/rootsoftheheart.jpeg",
                },
                new()
                {
                    Title = "Manga Time",
                    Description = "A girl who dreams of becoming a manga artist struggles with her lack of talent, while her reclusive classmate excels effortlessly. Their lives intertwine through a mysterious time-traveling adventure that challenges their perceptions of art and destiny.",
                    ReleaseYear = 2014,
                    Rating = "PG",
                    RunningTime = 129,
                    Genres = ["Comedy"],
                    Starring = ["Yamamoto Riku", "Ito Ren"],
                    Poster = "images/posters/mangatime.jpeg",
                },
                new()
                {
                    Title = "American Dream, Russian Twist",
                    Description = "A girl in the USA, chasing the American dream, marries a quirky young man with wealthy Russian parents. Their chaotic wedding in Las Vegas turns into a wild search for the missing groom.",
                    ReleaseYear = 2018,
                    Rating = "R",
                    RunningTime = 127,
                    Genres = ["Drama"],
                    Starring = ["Ava Wilson", "Alexei Kuznetsov", "Sergei Volkov"],
                    Poster = "images/posters/americandreamrussiantwist.jpeg",
                },
                new()
                {
                    Title = "Book Talk",
                    Description = "An old man finds joy in engaging door-to-door book sellers in detailed discussions about their books. His quirky and humorous interactions lead to unexpected friendships and hilarious moments.",
                    ReleaseYear = 1935,
                    Rating = "PG-13",
                    RunningTime = 129,
                    Genres = ["Comedy"],
                    Starring = ["Alexander Scott", "Amelia Young", "Mia Clark"],
                    Poster = "images/posters/booktalk.jpeg",
                },
                new()
                {
                    Title = "Empire Shadow",
                    Description = "A gladiator's journey to become the Roman Emperor is fraught with danger as he navigates a web of conspiracies and betrayal. The film delves into the intense political intrigue and power struggles of ancient Rome.",
                    ReleaseYear = 2007,
                    Rating = "R",
                    RunningTime = 115,
                    Genres = ["Action"],
                    Starring = ["Jack Thompson", "Olivia Brown", "Noah Miller"],
                    Poster = "images/posters/empireshadow.jpeg",
                },
                new()
                {
                    Title = "Tall World",
                    Description = "In a world where everything is towering, from skyscrapers to trees, humanity must adapt to new heights. Experience the awe and challenges of living in a land where the sky is no longer the limit.",
                    ReleaseYear = 2025,
                    Rating = "TV-MA",
                    RunningTime = 120,
                    Genres = ["Drama"],
                    Starring = ["Lucas Lewis", "Henry King", "Sophia Anderson"],
                    Poster = "images/posters/tallworld.jpeg",
                },
                new()
                {
                    Title = "The Mansion Ruse",
                    Description = "A kind-hearted lady is hired to clean a luxurious mansion, only to discover it's a setup by her employer and a scammer to drive her mad and steal her inheritance.",
                    ReleaseYear = 1998,
                    Rating = "PG-13",
                    RunningTime = 152,
                    Genres = ["Romance", "Comedy"],
                    Starring = ["Anastasia Ivanova", "Ivan Petrov", "Irina Pavlova"],
                    Poster = "images/posters/themansionruse.jpeg",
                },
                new()
                {
                    Title = "Tokyo Clean",
                    Description = "In the heart of Tokyo, a humble janitor finds beauty and dignity in his daily routine of cleaning toilets. This slice of life film explores the quiet resilience and unexpected joys of an ordinary man's life.",
                    ReleaseYear = 1968,
                    Rating = "PG",
                    RunningTime = 124,
                    Genres = ["Drama"],
                    Starring = ["Sato Haruki", "Kobayashi Mei"],
                    Poster = "images/posters/tokyoclean.jpeg",
                },
            }
        );
    }

    public Movie? GetMovieFromTitle(string title)
    {
        foreach (Movie movie in RegisteredMovies)
        {
            if (movie.Title.Equals(title, StringComparison.InvariantCultureIgnoreCase))
            {
                return movie;
            }
        }

        return null;
    }

    public List<Movie> GetAllMovies()
    {
        return this.RegisteredMovies;
    }
}
