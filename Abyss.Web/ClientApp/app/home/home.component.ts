import { Component } from "@angular/core";

import { AppService } from "../app.service";

@Component({
    templateUrl: "./home.template.html",
    styleUrls: ["./home.style.scss"],
})
export class HomeComponent {

    public appName: string = "Abyss";
    public get username() {
        const user = this.appService.getUser();
        if (user) {
            return user.Name;
        } else {
            return undefined;
        }
    }
    public count: number = 0;
    public values: string[] = [];
    constructor(public appService: AppService) { }
}
