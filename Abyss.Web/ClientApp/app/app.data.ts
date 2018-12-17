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
    ServerManager = "ServerManager",
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
