const apiUrl = '/api/bike';

export const getBikes = () => {
    return fetch(apiUrl)
        .then((res) => res.json())
}

export const getBikeById = (id) => {
    return fetch(`${apiUrl}/${id}`
    ).then((res) => {
        if (res.ok) {
            return res.json();
        } else {
            throw new Error(
                "An unknown error occured while trying to get post.",
            );
        }
    });
}

export const getBikesInShopCount = () => {
    return fetch(`${apiUrl}/getCount`)
        .then(res => res.json())
}