import { Component } from "@angular/core";

import { AuthService } from "../services/auth.service";

@Component({
    templateUrl: "./home.template.html",
    styleUrls: ["./home.style.scss"],
})
export class HomeComponent {
    constructor(public authService: AuthService) { }
}
