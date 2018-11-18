export interface IAuthResult {
    Token: string;
}

export interface IUser {
    Id: string;
    Name: string;
    Authentication: {
        [key: string]: string;
    };
}

export interface IAuthScheme {
    Id: string;
    Name: string;
    ProfileUrl: string;
    IconUrl: string;
}
