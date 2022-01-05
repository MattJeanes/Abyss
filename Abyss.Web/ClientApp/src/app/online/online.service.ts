import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { ITeamSpeakChannel, ITeamSpeakClient } from '../app.data';

@Injectable()
export class OnlineService {
    constructor(private httpClient: HttpClient) { }
    public getClients(): Promise<ITeamSpeakClient[]> {
        return firstValueFrom(this.httpClient.get<ITeamSpeakClient[]>('/api/online/client'));
    }
    public getChannels(): Promise<ITeamSpeakChannel[]> {
        return firstValueFrom(this.httpClient.get<ITeamSpeakChannel[]>('/api/online/channel'));
    }
}
