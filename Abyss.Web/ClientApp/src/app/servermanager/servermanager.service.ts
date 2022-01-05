import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { IServer } from '../app.data';

@Injectable()
export class ServerManagerService {
    constructor(private httpClient: HttpClient) { }
    public getServers(): Promise<IServer[]> {
        return firstValueFrom(this.httpClient.get<IServer[]>('/api/server'));
    }
    public start(serverId: string): Promise<boolean> {
        return firstValueFrom(this.httpClient.post<boolean>(`/api/server/start/${serverId}`, undefined));
    }
    public stop(serverId: string): Promise<boolean> {
        return firstValueFrom(this.httpClient.post<boolean>(`/api/server/stop/${serverId}`, undefined));
    }
}
