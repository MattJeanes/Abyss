import { Component, OnInit } from "@angular/core";
import { TdDialogService } from "@covalent/core";

import { IServer, ServerStatus } from "../app.data";
import { ServerService } from "../services/server.service";

@Component({
    templateUrl: "./servermanager.template.html",
})
export class ServerManagerComponent implements OnInit {
    public servers: IServer[];
    public ServerStatus = ServerStatus;
    public selectedId?: string;
    public loading: boolean;

    public get selected() { return this.servers ? this.servers.find(x => x.Id === this.selectedId) : undefined; }

    constructor(public serverService: ServerService, public dialogService: TdDialogService) { }

    public async ngOnInit() {
        try {
            this.loading = true;
            await this.refresh();
        } catch (e) {
            console.error(e);
            this.dialogService.openAlert({
                title: "Failed to load server manager",
                message: e.toString(),
            });
        } finally {
            this.loading = false;
        }
    }

    public async start() {
        if (!this.selected || this.loading) { return; }
        try {
            this.loading = true;
            await this.serverService.start(this.selected.Id);

        } catch (e) {
            console.error(e);
            this.dialogService.openAlert({
                title: "Failed to start server",
                message: e.toString(),
            });
        } finally {
            this.loading = false;
            await this.refresh();
        }
    }

    public async stop() {
        if (!this.selected || this.loading) { return; }
        try {
            this.loading = true;
            await this.serverService.stop(this.selected.Id);
        } catch (e) {
            console.error(e);
            this.dialogService.openAlert({
                title: "Failed to stop server",
                message: e.toString(),
            });
        } finally {
            this.loading = false;
            await this.refresh();
        }
    }

    public async refresh() {
        const isLoading = this.loading;
        if (!isLoading) {
            this.loading = true;
        }
        try {
            this.servers = await this.serverService.getServers();
        } catch (e) {
            console.error(e);
            this.dialogService.openAlert({
                title: "Failed to refresh servers",
                message: e.toString(),
            });
        } finally {
            if (!isLoading) {
                this.loading = false;
            }
        }
    }
}
