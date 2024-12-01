import { Component, NgZone, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';

import { HttpTransportType, HubConnectionBuilder } from '@microsoft/signalr';

import { IServer, ServerStatus } from '../app.data';
import { ServerManagerService } from './servermanager.service';

import { DialogService } from '../services';


@Component({
    templateUrl: './servermanager.component.html',
    styleUrls: ['./servermanager.component.scss'],
    imports: [
        FormsModule,
        CommonModule,
        MatSelectModule,
        MatButtonModule,
        MatProgressSpinnerModule,
        MatIconModule,
    ],
    providers: [
        ServerManagerService
    ],
})
export class ServerManagerComponent implements OnInit, OnDestroy {
    public servers?: IServer[];
    public ServerStatus = ServerStatus;
    public selectedId?: string;
    public loading = false;
    public log: string[] = [];
    public hubReady = false;
    private hub = new HubConnectionBuilder()
        .withUrl('hub/servermanager', { transport: HttpTransportType.WebSockets })
        .withAutomaticReconnect()
        .build();

    public get selected(): IServer | undefined { return this.servers ? this.servers.find(x => x.Id === this.selectedId) : undefined; }

    constructor(public serverManagerService: ServerManagerService, public dialogService: DialogService, private ngZone: NgZone) {
        this.hub.on('status', (status: ServerStatus) => {
            this.ngZone.run(() => {
                let selectedServer = this.selected;
                if (!selectedServer) { return; }
                selectedServer.StatusId = status;
            });
        });
        this.hub.on('complete', (err: string) => {
            this.ngZone.run(() => {
                this.loading = false;
                if (err) {
                    this.dialogService.alert({
                        title: 'Failed to complete server operation',
                        message: err.toString(),
                    });
                }
            });
        });
        this.hub.on('log', (log: string) => {
            this.ngZone.run(() => {
                this.log.push(log);
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
                this.log.push(`Lost connection to server manager, attempting to reconnect...${err ? `\n${err}` : ''}`)
            });
        });
        this.hub.onreconnected(() => {
            this.ngZone.run(() => {
                this.hubReady = true;
                this.log.push(`Reconnected to server manager`);
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
            this.log = [];
            await this.serverManagerService.start(this.selected.Id, this.hub.connectionId!);
        } catch (e: any) {
            this.dialogService.alert({
                title: 'Failed to start server',
                message: e.toString(),
            });
        }
    }

    public async stop(): Promise<void> {
        if (!this.selected || this.loading || !this.hubReady) { return; }
        try {
            this.loading = true;
            this.log = [];
            await this.serverManagerService.stop(this.selected.Id, this.hub.connectionId!);
        } catch (e: any) {
            this.dialogService.alert({
                title: 'Failed to stop server',
                message: e.toString(),
            });
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
