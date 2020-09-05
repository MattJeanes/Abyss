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
    UserManager = 'UserManager',
    ErrorViewer = 'ErrorViewer',
    ServerManager = 'ServerManager',
    WhoSaid = 'WhoSaid',
    GPT = 'GPT',
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

export interface IGPTMessage {
    text: string;
    human?: boolean;
    model?: GPTModel;
}

export enum GPTModel {
    Boys = 'Boys',
    DanBot = 'DanBot',
}