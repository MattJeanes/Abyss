import { Component, NgZone, OnInit, OnDestroy } from '@angular/core';
import { HttpTransportType, HubConnectionBuilder } from '@microsoft/signalr';

import { IServer, ServerStatus } from '../app.data';
import { ServerManagerService } from './servermanager.service';

import { DialogService } from '../services';

@Component({
    templateUrl: './servermanager.component.html',
})
export class ServerManagerComponent implements OnInit {
    public servers?: IServer[];
    public ServerStatus = ServerStatus;
    public selectedId?: string;
    public loading = false;
    public log = '';
    public hubReady = false;
    private hub = new HubConnectionBuilder()
        .withUrl('hub/servermanager', { transport: HttpTransportType.WebSockets })
        .withAutomaticReconnect()
        .build();

    public get selected(): IServer | undefined { return this.servers ? this.servers.find(x => x.Id === this.selectedId) : undefined; }

    constructor(public serverManagerService: ServerManagerService, public dialogService: DialogService, private ngZone: NgZone) {
        this.hub.on('update', (log: string) => {
            // Because this is a call from the server, Angular change detection won't detect it so we must force ngZone to run
            this.ngZone.run(() => {
                this.log = log;
            });
        });
        this.hub.onclose(() => {
            this.ngZone.run(() => {
                this.hubReady = false;
            });
        });
        this.hub.onreconnecting((err) => {
            this.ngZone.run(() => {
                this.hubReady = false;
            });
        });
        this.hub.onreconnected(() => {
            this.ngZone.run(() => {
                this.hubReady = true;
            });
        });
    }

    public async ngOnInit(): Promise<void> {
        try {
            this.loading = true;
            await this.refresh();
            await this.hub.start();
            this.hubReady = true;
        } catch (e: any) {
            this.dialogService.alert({
                title: 'Failed to load server manager',
                message: e.toString(),
            });
        } finally {
            this.loading = false;
        }
    }

    public ngOnDestroy(): void {
        if (this.hubReady) {
            this.hub.stop();
        }
    }

    public async start(): Promise<void> {
        if (!this.selected || this.loading || !this.hubReady) { return; }
        try {
            this.loading = true;
            await this.serverManagerService.start(this.selected.Id, this.hub.connectionId!);
        } catch (e: any) {
            this.dialogService.alert({
                title: 'Failed to start server',
                message: e.toString(),
            });
        } finally {
            this.loading = false;
            await this.refresh();
        }
    }

    public async stop(): Promise<void> {
        if (!this.selected || this.loading || !this.hubReady) { return; }
        try {
            this.loading = true;
            await this.serverManagerService.stop(this.selected.Id, this.hub.connectionId!);
        } catch (e: any) {
            this.dialogService.alert({
                title: 'Failed to stop server',
                message: e.toString(),
            });
        } finally {
            this.loading = false;
            await this.refresh();
        }
    }

    public async refresh(): Promise<void> {
        const isLoading = this.loading;
        if (!isLoading) {
            this.loading = true;
        }
        try {
            this.servers = await this.serverManagerService.getServers();
        } catch (e: any) {
            this.dialogService.alert({
                title: 'Failed to refresh servers',
                message: e.toString(),
            });
        } finally {
            if (!isLoading) {
                this.loading = false;
            }
        }
    }
}
