// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Samples.NotFlix;

public class Movie
{
    /// <summary>
    /// Movie summary.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Movie generes.
    /// </summary>
    public string[] Genres { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Movie duration in minutes.
    /// </summary>
    public int RunningTime { get; set; } = 0;

    /// <summary>
    /// Movie poster.
    /// </summary>
    public string Poster { get; set; } = string.Empty;

    /// <summary>
    /// Movie rating.
    /// </summary>
    public string Rating { get; set; } = string.Empty;

    /// <summary>
    /// Movie release year.
    /// </summary>
    public int ReleaseYear { get; set; } = 2000;

    /// <summary>
    /// List of actors starring in the movie.
    /// </summary>
    public string[] Starring { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Movie title.
    /// </summary>
    public string Title { get; set; } = string.Empty;
}
