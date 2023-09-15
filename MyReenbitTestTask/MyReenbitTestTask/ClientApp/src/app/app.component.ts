import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  currUrl: string = "";
  http: HttpClient;
  emailError: boolean = false;
  fileError: boolean = false;
  selectedFile: File | null = null;
  email: string = '';
  isSent: boolean = false;
  sendMessage: string = "";

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.currUrl = baseUrl + 'fileupload';
    this.http = http;
  }

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
  }

  onSubmit() {
    this.emailError = false;
    this.fileError = false;

    if (!this.isValidEmail(this.email)) {
      this.emailError = true;
      return;
    }

    if (!this.selectedFile) {
      this.fileError = true;
      return;
    }

    // Create a FormData object to send the file and email
    const formData = new FormData();
    formData.append('file', this.selectedFile);
    formData.append('email', this.email);

    this.http.post(this.currUrl, formData, { responseType: 'json' })
      .subscribe(response => {
        console.log(response);
        this.sendMessage = "file have been sended";
      }, error => {
        console.error(error);
        this.sendMessage = "send fail";
      });
    this.isSent = true;
  }

  private isValidEmail(email: string): boolean {
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
    return emailRegex.test(email);
  }
}
