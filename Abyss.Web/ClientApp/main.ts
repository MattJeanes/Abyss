// Main

import "./styles/main.scss";

import "./polyfills";

import "hammerjs";

import { enableProdMode } from "@angular/core";
import { platformBrowserDynamic } from "@angular/platform-browser-dynamic";
import { AppModule } from "./app/app.module";

declare var module: any;

if (module.hot) {
    module.hot.accept();
    module.hot.dispose(() => {
        // Before restarting the app, we create a new root element and dispose the old one
        const oldRootElem = document.querySelector("abyss");
        const newRootElem = document.createElement("abyss");
        if (oldRootElem && oldRootElem.parentNode) {
            oldRootElem.parentNode.insertBefore(newRootElem, oldRootElem);
            oldRootElem.parentNode.removeChild(oldRootElem);
        }
        if (modulePromise) {
            modulePromise.then(appModule => appModule.destroy());
        }
    });
} else {
    enableProdMode();
}

const modulePromise = platformBrowserDynamic().bootstrapModule(AppModule);
