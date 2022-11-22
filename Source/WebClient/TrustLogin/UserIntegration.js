class UserIntegration {

    #config;

    constructor(environmentFile) {
        this.#config = JSON.parse(environmentFile);
    };

    Login = async (email, password) => {
        await fetch(this.#config['ip'] + this.#config['base_url'] + "login", {
            method: 'POST',
            body: {'Email': email, 'Password': password}
        }).then(response => {
            if (response.ok)
                return response.json;

            throw new Error("Request failed!");
        }, networkError => console.log(networkError.message)
        ).then(jsonResponse =>
             console.log(jsonResponse))
    };

}