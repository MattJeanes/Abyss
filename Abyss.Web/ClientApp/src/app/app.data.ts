export interface IAuthResult {
    Token: string;
}

export interface IClientUserAuthentication {
    [key: number]: string;
}

export interface IClientUser {
    Id: string;
    Name: string;
    Authentication: IClientUserAuthentication;
    Permissions: string[];
}

export interface IUser {
    Name: string;
}

export interface IToken {
    User: string;
    RefreshExpiry: string;
    exp: number;
}

export interface IAuthScheme {
    Id: number;
    Name: string;
    ProfileUrl: string;
    IconUrl: string;
}

export enum Permissions {
    UserManager = 'UserManager',
    ErrorViewer = 'ErrorViewer',
    ServerManager = 'ServerManager',
    WhoSaid = 'WhoSaid',
}

export enum ServerStatus {
    Active,
    Inactive,
    Activating,
    Deactivating,
}

export enum AuthSchemeType {
    Steam = 0,
    Google = 1,
    Discord = 2
}

export interface IServer {
    Id: string;
    Name: string;
    StatusId: ServerStatus;
    IPAddress?: string;
}

export interface ITeamSpeakClient {
    Name: string;
    ChannelId: number;
    ConnectedSeconds: number;
    IdleSeconds: number;
}

export interface ITeamSpeakChannel {
    Id: number;
    Name: string;
    ParentId: number;
}

export interface IWhoSaid {
    Name: string;
    Message: string;
}

export interface IGPTRequest {
    Text: string;
    ModelId: string;
    Temperature: number;
    TopP: number;
}

export interface IGPTResponse {
    Text: string;
}

export interface IGPTMessage {
    Text: string;
    Human?: boolean;
}

export interface IGPTModel {
    Id: string;
    Name: string;
    Identifier: string;
}

export interface ICommonDialog {
    title: string;
    message: string;
    width?: string;
}

export interface IDialogAlert extends ICommonDialog {
    closeButtonText?: string;
}

export interface IDialogConfirm extends ICommonDialog {
    confirmButtonText?: string;
    cancelButtonText?: string;
}

export interface IDialogPrompt extends ICommonDialog {
    acceptButtonText?: string;
    cancelButtonText?: string;
    value?: string;
}