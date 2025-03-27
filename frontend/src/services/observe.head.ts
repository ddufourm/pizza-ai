import { Injectable } from '@angular/core';
import { environment } from '../environments/environment';

@Injectable({
	providedIn: 'root',
})
export class StyleObserverService {
	constructor() {
		this.monkeyPatchAppendChild();
	}

	private monkeyPatchAppendChild() {
		const originalAppendChild = HTMLHeadElement.prototype.appendChild;

		HTMLHeadElement.prototype.appendChild = function <T extends Node>(newChild: T): T {
			if (newChild instanceof HTMLStyleElement) {
				if (!newChild.nonce) {
					newChild.nonce = environment.cspNonce;
				}
			}
			return originalAppendChild.call(this, newChild) as T;
		};
	}

	ngOnDestroy() {}
}
