import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { HttpClient } from '@angular/common/http';

import { environment } from '../environments/environment';

@Component({
	selector: 'app-root',
	standalone: true,
	imports: [
		CommonModule,
		RouterOutlet,
		ReactiveFormsModule,
		MatFormFieldModule,
		MatInputModule,
		MatButtonModule,
	],
	templateUrl: './app.component.html',
	styleUrl: './app.component.scss',
})
export class AppComponent {
	title = 'PizaAI';

	form: FormGroup;

	constructor(private fb: FormBuilder, private http: HttpClient) {
		this.form = this.fb.group({
			textInput: ['', [Validators.required, Validators.maxLength(30)]],
		});
	}

	onSubmit() {
		if (this.form.valid) {
			const emotionData = {
				text: this.form.get('textInput')?.value,
				timestamp: new Date().toISOString(),
			};

			this.http
				.post<any>(`${environment.apiUrl}/pizza/pizza-suggestion`, emotionData)
				.subscribe({
					next: (response) => {
						console.log('Réponse du backend:', response);
						// Traiter la réponse (redirection/affichage)
					},
					error: (err) => {
						console.error('Erreur:', err);
						// Gérer les erreurs
					},
				});
		}
	}
}
