import { Component, OnInit } from '@angular/core';

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

    public get selected(): IServer | undefined { return this.servers ? this.servers.find(x => x.Id === this.selectedId) : undefined; }

    constructor(public serverManagerService: ServerManagerService, public dialogService: DialogService) { }

    public async ngOnInit(): Promise<void> {
        try {
            this.loading = true;
            await this.refresh();
        } catch (e: any) {
            this.dialogService.alert({
                title: 'Failed to load server manager',
                message: e.toString(),
            });
        } finally {
            this.loading = false;
        }
    }

    public async start(): Promise<void> {
        if (!this.selected || this.loading) { return; }
        try {
            this.loading = true;
            await this.serverManagerService.start(this.selected.Id);
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
        if (!this.selected || this.loading) { return; }
        try {
            this.loading = true;
            await this.serverManagerService.stop(this.selected.Id);
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
