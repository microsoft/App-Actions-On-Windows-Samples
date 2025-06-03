// Copyright(C) Microsoft Corporation.All rights reserved.

// The cookies are stored in a single string split by ';' characters.
// i.e. watchlist=A|B|C; path=/

// Gets the cookies stored in the browser and creates a dictionary out of them.
function getCookieDictionary() {
    const dictionary = [];
    const cookieString = decodeURIComponent(document.cookie);

    if (cookieString.length != 0) {
        const cookies = cookieString.split(';');
        for (let i = 0; i < cookies.length; i++) {
            cookie = cookies[i];
            cookie = cookie.trim();
            const keyValue = cookie.split('=');
            dictionary[keyValue[0]] = keyValue[1];
        }
    }

    return dictionary;
}

// Converts a cookie dictionary to the proposed cookie string format and stores it.
function saveCookies(dictionary) {
    let cookieString = '';
    for (let key in dictionary) {
        if (key !== 'path') {
            cookieString = cookieString + key + "=" + dictionary[key] + "; ";
        }
    }

    cookieString = cookieString + "path=/";
    document.cookie = cookieString;
}

// Adds a movie to the watchlist cookie. If the movie is already in the watchlist, it won't be added again.
function addMovieToWatchListCookie(movie) {
    const cookies = getCookieDictionary();
    let watchlist = cookies['watchlist'];
    const separator = '|';

    if (watchlist) {
        let alreadyAdded = false;
        const moviesInWatchlist = watchlist.split(separator);

        for (let i = 0; i < moviesInWatchlist.length; i++) {
            if (moviesInWatchlist[i] === movie) {
                alreadyAdded = true;
                break;
            }
        }

        if (!alreadyAdded) {
            watchlist = watchlist + separator + movie;
        }
    }
    else {
        watchlist = movie;
    }

    cookies['watchlist'] = watchlist;
    saveCookies(cookies);
}

// Retrieves the watchlist cookie. If the watchlist cookie is not set, returns an empty string.
function getWatchListCookie() {
    const cookies = getCookieDictionary();
    const watchList = cookies['watchlist'];
    if (watchList == undefined || watchList == null) {
        return "";
    }

    return watchList;
}
