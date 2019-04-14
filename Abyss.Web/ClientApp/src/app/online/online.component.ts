import { Component, NgZone, OnDestroy, OnInit } from "@angular/core";
import { HttpTransportType, HubConnectionBuilder } from "@aspnet/signalr";
import { TdDialogService } from "@covalent/core";

import { ITeamSpeakChannel, ITeamSpeakClient } from "../app.data";
import { OnlineService } from "../services/online.service";

@Component({
    templateUrl: "./online.template.html",
    styleUrls: ["./online.style.scss"],
})
export class OnlineComponent implements OnInit, OnDestroy {
    public clients: ITeamSpeakClient[] = [];
    public channels: ITeamSpeakChannel[] = [];
    public loading = false;
    private hub = new HubConnectionBuilder()
        .withUrl("hub/online", { transport: HttpTransportType.WebSockets })
        .build();
    private hubReady = false;
    constructor(public onlineService: OnlineService, public dialogService: TdDialogService, private ngZone: NgZone) {
        this.hub.on("update", (clients: ITeamSpeakClient[], channels: ITeamSpeakChannel[]) => {
            // Because this is a call from the server, Angular change detection won't detect it so we must force ngZone to run
            this.ngZone.run(() => {
                this.channels = channels;
                this.clients = clients;
            });
        });
    }

    public async ngOnInit() {
        try {
            this.loading = true;
            await this.refresh();
            await this.hub.start();
            this.hubReady = true;
        } catch (e) {
            this.dialogService.openAlert({
                title: "Failed to load online",
                message: e.toString(),
            });
        } finally {
            this.loading = false;
        }
    }

    public ngOnDestroy() {
        if (this.hubReady) {
            this.hub.stop();
        }
    }

    public async refresh() {
        const isLoading = this.loading;
        if (!isLoading) {
            this.loading = true;
        }
        try {
            this.channels = await this.onlineService.getChannels();
            this.clients = await this.onlineService.getClients();
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

    public getChannelName(id: number) {
        try {
            const channel = this.channels.find(x => x.Id === id);
            let channelName = channel.Name;
            if (channel.ParentId) {
                channelName += ` / ${this.getChannelName(channel.ParentId)}`;
            }
            return channelName;
        } catch (e) {
            console.error(e);
            return "Error";
        }
    }
}
