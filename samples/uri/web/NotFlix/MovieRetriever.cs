// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Samples.NotFlix;

/// <summary>
/// 
/// </summary>
public class MovieRetriever
{
    private MovieDatabase Database;

    public MovieRetriever()
    {
        this.Database = new MovieDatabase();
    }

    public Movie? GetMovieFromTitle(string title)
    {
        return this.Database.GetMovieFromTitle(title);
    }

    public List<Movie> GetAllMovies()
    {
        return this.Database.GetAllMovies();
    }
}
