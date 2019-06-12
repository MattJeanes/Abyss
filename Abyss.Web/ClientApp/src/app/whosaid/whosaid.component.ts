import { Component } from "@angular/core";

import { WhoSaidService } from "../services/whosaid.service";

@Component({
    templateUrl: "./whosaid.template.html",
    styleUrls: ["./whosaid.style.scss"],
})
export class WhoSaidComponent {
    public name = "Someone";

    constructor(private whoSaidService: WhoSaidService) { }

    public async whoSaid(message: string) {
        const whoSaid = await this.whoSaidService.whoSaid(message);
        this.name = whoSaid.Name;
    }
}
