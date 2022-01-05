export interface IAuthResult {
    Token: string;
}

export interface IClientUserAuthentication {
    [key: string]: string;
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
    Id: string;
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

export interface IServer {
    Id: string;
    Tag: string;
    SnapshotId?: number;
    Size?: string;
    Region?: string;
    DropletId?: number;
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

export interface IDialogAlert {
    title: string;
    message: string;
}

export interface IDialogConfirm {
    title: string;
    message: string;
    acceptButton: string;
    cancelButton: string;
}

export interface IDialogPrompt {
    title: string;
    message: string;
    value: string | undefined;
}