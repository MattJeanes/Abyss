import { Component } from "@angular/core";

import { AppService } from "../app.service";

@Component({
    templateUrl: "./home.template.html",
    styleUrls: ["./home.style.scss"],
})
export class HomeComponent {

    public appName: string = "Abyss";
    public count: number = 0;
    public values: string[] = [];
    constructor(public appService: AppService) { }
    public loginOrOut() {
        if (this.appService.isLoggedIn()) {
            this.appService.logout();
        } else {
            this.appService.login();
        }
    }
}
