import { Component, OnInit } from "@angular/core";
import { TdDialogService } from "@covalent/core";

import { ITeamSpeakChannel, ITeamSpeakClient } from "../app.data";
import { OnlineService } from "../services/online.service";

@Component({
    templateUrl: "./online.template.html",
    styleUrls: ["./online.style.scss"],
})
export class OnlineComponent implements OnInit {
    public clients: ITeamSpeakClient[];
    public channels: ITeamSpeakChannel[];
    public loading: boolean = false;
    constructor(public onlineService: OnlineService, public dialogService: TdDialogService) { }

    public async ngOnInit() {
        try {
            this.loading = true;
            await this.refresh();
        } catch (e) {
            this.dialogService.openAlert({
                title: "Failed to load online",
                message: e.toString(),
            });
        } finally {
            this.loading = false;
        }
    }

    public async refresh() {
        const isLoading = this.loading;
        if (!isLoading) {
            this.loading = true;
        }
        try {
            this.clients = await this.onlineService.getClients();
            this.channels = await this.onlineService.getChannels();
        } catch (e) {
            this.dialogService.openAlert({
                title: "Failed to refresh",
                message: e.toString(),
            });
        } finally {
            if (!isLoading) {
                this.loading = false;
            }
        }
    }
}
