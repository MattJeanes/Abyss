export interface IAuthResult {
    Token: string;
}

export interface IClientUser {
    Id: string;
    Name: string;
    Authentication: {
        [key: string]: string;
    };
    Permissions: string[];
}

export interface IUser {
    // todo
    Name: string;
}

export interface IToken {
    User: string;
    RefreshExpiry: string;
    exp: number;
}

export interface IAuthScheme {
    Id: string;
    Name: string;
    ProfileUrl: string;
    IconUrl: string;
}

export enum Permissions {
    UserManager = "UserManager",
    ErrorViewer = "ErrorViewer",
}
