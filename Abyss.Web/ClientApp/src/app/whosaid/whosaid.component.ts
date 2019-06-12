import { Component } from "@angular/core";

import { IWhoSaid } from "../app.data";
import { WhoSaidService } from "../services/whosaid.service";

@Component({
    templateUrl: "./whosaid.template.html",
    styleUrls: ["./whosaid.style.scss"],
})
export class WhoSaidComponent {
    public name = "Someone";
    public log: IWhoSaid[] = [];

    constructor(private whoSaidService: WhoSaidService) { }

    public async whoSaid(message: string) {
        if (!message) { return; }
        const whoSaid = await this.whoSaidService.whoSaid(message);
        this.log = [...this.log, whoSaid];
        this.name = whoSaid.Name;
    }

    public clear() {
        this.log = [];
    }

    public undo() {
        this.log.pop();
    }
}
