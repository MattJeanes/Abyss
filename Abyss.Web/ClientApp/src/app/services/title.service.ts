import { Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';

export const TITLE_PREFIX = 'Abyss - ';

@Injectable()
export class TitleService {
    constructor(private title: Title) {}

    getTitle(): string {
        return this.title.getTitle();
    }

    setTitle(newTitle: string, includePrefix: boolean = true): void {
        const title = includePrefix && !newTitle.startsWith(TITLE_PREFIX) ? TITLE_PREFIX + newTitle : newTitle;
        this.title.setTitle(title);
    }
}
