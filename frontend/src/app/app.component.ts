/**
 * External dependencies
 */
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { HttpClient, HttpHeaders } from '@angular/common/http';

/**
 * Internal dependencies
 */
import { environment } from '../environments/environment';
import { Pizza } from '../models/pizza.model';

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
	title = 'PizzaAI';
	form: FormGroup;
	responseMessage: string = '';
	pizza: Pizza = new Pizza();
	isLoading: boolean = false;

	constructor(private fb: FormBuilder, private http: HttpClient) {
		this.form = this.fb.group({
			textInput: ['', [Validators.required, Validators.maxLength(30)]],
		});
	}

	onSubmit() {
		if (this.form.valid) {
			this.isLoading = true;
			this.responseMessage = '';
			const emotionData = {
				text: this.form.get('textInput')?.value,
				timestamp: new Date().toISOString(),
			};

			console.log(environment);
			this.http
				.post<any>(`${environment.apiUrl}/pizza/pizza-suggestion`, emotionData, {
					headers: new HttpHeaders({
						'Content-Type': 'application/json',
						'X-Requested-With': 'XMLHttpRequest',
					}),
				})
				.subscribe({
					next: (response) => {
						console.log('Réponse du backend:', response);
						this.pizza = response.pizza;
						this.isLoading = false;
					},
					error: (err) => {
						console.error('Erreur:', err);
						this.responseMessage =
							"Une erreur s'est produite lors de la suggestion de pizza.";
						this.isLoading = false;
					},
				});
		}
	}
}
