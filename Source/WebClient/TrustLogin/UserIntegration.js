class UserIntegration {

    // The config file with information about server details.
    #config;

    // Error event called when a request fails. (message, code)
    onErrorEvent;

    // Constructor for integration takes a environmnt file.
    constructor(environmentFile) {
        this.#config = environmentFile;
    };

    // Calls the onErrorEvent if method attached.
    #callOnErrorEvent = (message, code) => {
        if (this.onErrorEvent != null || this.onErrorEvent != undefined)
            this.onErrorEvent(message, code);
    };

    // Create basic fetch promise with config details.
    #createFetch = async (body, method, endpoint) => {
        return await fetch(this.#config['ip'] + this.#config['base_url'] + endpoint, {
            method: method,
            headers: new Headers({'content-type': 'application/json'}),
            body: JSON.stringify(body)
        });
    };

    // Calls the api to login an user with the given email & password.
    login = async (email, password) => {
        let bodyRequest = {Email: email, Password: password};
        
        return await this.#createFetch(bodyRequest, 'POST', "login")
        .then(response => {
            if (response.status == 200)
                return response.json();
            
            // Error handling
            response.json().then(json => this.#callOnErrorEvent(json['error'], response.status));
        }, networkError => this.#callOnErrorEvent("Network error: " + networkError.message, 500)
        );
    };

    // Calls the api to register an user with the given details.
    register = async (email, password, firstname, lastname, phonenumber) => {
        let bodyRequest = {email: email, password: password, firstname: firstname, lastname: lastname, phonenumber: phonenumber};

        return await this.#createFetch(bodyRequest, 'POST', 'create').then(response => {
            if (response.status == 200)
                return response.json();
            
            // Error handling
            response.json().then(json => this.#callOnErrorEvent(json['error'], response.status));
        }, networkError => this.#callOnErrorEvent("Network error: " + networkError.message, 500)
        );
    };

    verifyLogin = async (email, password) => {
        let bodyRequest = {email: email, password: password};

        return await this.#createFetch(bodyRequest, 'POST', 'verificationlogin').then(response => {
            if (response.status == 200)
            return response.json();
        
        // Error handling
        response.json().then(json => this.#callOnErrorEvent(json['error'], response.status));
        }, networkError => this.#callOnErrorEvent("Network error: " + networkError.message, 500)
        );
    };

}