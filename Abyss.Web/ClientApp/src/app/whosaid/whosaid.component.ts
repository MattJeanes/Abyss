import { Component } from "@angular/core";
import { TdDialogService } from "@covalent/core/dialogs";

import { IWhoSaid } from "../app.data";
import { WhoSaidService } from "../services/whosaid.service";

@Component({
    templateUrl: "./whosaid.template.html",
    styleUrls: ["./whosaid.style.scss"],
})
export class WhoSaidComponent {
    public name = "Someone";
    public log: IWhoSaid[] = [];
    public loading = false;
    public message = "";

    constructor(private whoSaidService: WhoSaidService, private dialogService: TdDialogService) { }

    public async whoSaid() {
        try {
            if ((!this.message) || this.loading) { return; }
            this.loading = true;
            const whoSaid = await this.whoSaidService.whoSaid(this.message);
            this.log = [...this.log, whoSaid];
            this.name = whoSaid.Name;
        } catch (e) {
            this.dialogService.openAlert({
                title: "Failed to get who said it",
                message: e.toString(),
            });
        } finally {
            this.loading = false;
        }
    }

    public clear() {
        this.log = [];
    }

    public undo() {
        this.log.pop();
    }
}
