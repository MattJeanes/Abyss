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
    public start(serverId: string, connectionId: string): Promise<boolean> {
        const params = connectionId ? { connectionId } : undefined;
        return firstValueFrom(this.httpClient.post<boolean>(`/api/server/start/${serverId}`, undefined, { params }));
    }
    public stop(serverId: string, connectionId: string): Promise<boolean> {
        const params = connectionId ? { connectionId } : undefined;
        return firstValueFrom(this.httpClient.post<boolean>(`/api/server/stop/${serverId}`, undefined, { params }));
    }
    public executeCommand(serverId: string, command: string): Promise<string> {
        return firstValueFrom(this.httpClient.post<string>(`/api/server/command/${serverId}`, JSON.stringify(command), {
            headers: { 'Content-Type': 'application/json' },
            responseType: 'text' as 'json',
        }));
    }
}
