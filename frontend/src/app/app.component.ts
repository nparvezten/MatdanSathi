import { Component, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FormWizardComponent } from './components/form-wizard/form-wizard.component';
import { BloMapComponent } from './components/blo-map/blo-map.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, FormWizardComponent, BloMapComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  title = 'MatdanSathi Dashboard';
  activeTab = signal<'public' | 'volunteer'>('public');
  isAuthenticated = signal<boolean>(false);
  email = '';
  password = '';
  errorMessage = signal<string>('');
  isLoading = signal<boolean>(false);
  verifierEmail = signal<string>('');

  // Public Tool 1: Legacy EPIC Decoder
  legacyEpicInput = '';
  decodedResult = signal<any>(null);

  // Public Tool 2: Delimitation Time Machine
  selectedConstituency = '';
  timeMachineResult = signal<any>(null);

  // Public Tool 3: Marathi Transliterator
  englishNameInput = '';
  transliteratedName = signal<string>('');

  // Public Tool 4: Volunteer Sign Up
  regName = '';
  regEmail = '';
  regPhone = '';
  regAssembly = '';
  registrationSuccess = signal<boolean>(false);

  ngOnInit() {
    const token = localStorage.getItem('auth_token');
    const savedEmail = localStorage.getItem('verifier_email');
    if (token) {
      this.isAuthenticated.set(true);
      this.verifierEmail.set(savedEmail || 'verifier@matdansathi.org');
    }
  }

  setTab(tab: 'public' | 'volunteer') {
    this.activeTab.set(tab);
  }

  decodeLegacyEpic() {
    const input = this.legacyEpicInput.trim().toUpperCase();
    const parts = input.split('/');
    if (parts.length >= 4) {
      const stateMap: Record<string, string> = {
        'MT': 'Maharashtra',
        'DL': 'Delhi',
        'GJ': 'Gujarat',
        'KA': 'Karnataka',
        'MH': 'Maharashtra'
      };
      const stateCode = parts[0];
      const state = stateMap[stateCode] || `State Code: ${stateCode}`;
      this.decodedResult.set({
        isValid: true,
        state: state,
        lokSabha: `Lok Sabha Constituency No: ${parts[1]}`,
        assemblyPart: `Assembly Constituency / Part No: ${parts[2]}`,
        serialNo: `Voter Serial Number: ${parts[3]}`,
        suggestion: `Search historical ${state} archives under Segment ${parts[2]}, Serial ${parts[3]}.`
      });
    } else {
      this.decodedResult.set({
        isValid: false,
        message: 'Invalid legacy format. Use standard slash-separated code like MT/05/025/180293.'
      });
    }
  }

  onConstituencyChange() {
    const mappings: Record<string, any> = {
      'sion': {
        modern: 'Sion Koliwada (Ward 179 limits)',
        historical: 'Matunga Constituency (Old Constituency limits)',
        ward: 'BMC F-North Ward Office (Marginal Boundary Limits)'
      },
      'bandra': {
        modern: 'Vandre West / Bandra (Ward 98 limits)',
        historical: 'Amboli / Jogeshwari limits (Historical Boundary)',
        ward: 'BMC K-West Ward Office (Paliram Road)'
      },
      'pune': {
        modern: 'Shivajinagar (Ward 12 limits)',
        historical: 'Pune Cantonment (Historical Ward limits)',
        ward: 'PMC Pune Cantonment Board Office'
      },
      'byculla': {
        modern: 'Chinchpokli / Byculla (Ward 204 limits)',
        historical: 'Mazgaon Constituency (Old Ward Limits)',
        ward: 'BMC E-Ward Office (Byculla)'
      }
    };
    this.timeMachineResult.set(mappings[this.selectedConstituency] || null);
  }

  transliterate() {
    const input = this.englishNameInput.trim().toLowerCase();
    if (!input) {
      this.transliteratedName.set('');
      return;
    }
    const words = input.split(/\s+/);
    const mapping: Record<string, string> = {
      'saidnabi': 'सईदनबी',
      'khan': 'खान',
      'imran': 'इमरान',
      'shaikh': 'शेख',
      'sheikh': 'शेख',
      'farida': 'फरीदा',
      'begum': 'बेगम',
      'ramesh': 'रमेश',
      'sawant': 'सावंत',
      'saraswati': 'सरस्वती',
      'deepa': 'दीपा',
      'joshi': 'जोशी'
    };
    const mapped = words.map(w => mapping[w] || `[${w}]`);
    this.transliteratedName.set(mapped.join(' '));
  }

  registerVolunteer() {
    if (this.regName && this.regEmail && this.regPhone) {
      this.registrationSuccess.set(true);
      // Reset signup inputs
      setTimeout(() => {
        this.regName = '';
        this.regEmail = '';
        this.regPhone = '';
        this.regAssembly = '';
      }, 500);
    }
  }

  resetRegistration() {
    this.registrationSuccess.set(false);
  }

  async login() {
    this.isLoading.set(true);
    this.errorMessage.set('');

    try {
      const response = await fetch('/api/v1/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          email: this.email,
          password: this.password
        })
      });

      this.isLoading.set(false);

      if (response.ok) {
        const data = await response.json();
        localStorage.setItem('auth_token', data.token);
        localStorage.setItem('verifier_email', this.email);
        this.verifierEmail.set(this.email);
        this.isAuthenticated.set(true);
      } else {
        const errData = await response.json().catch(() => null);
        this.errorMessage.set(errData?.message || 'Invalid verifier email or password.');
      }
    } catch (err) {
      console.error(err);
      this.isLoading.set(false);
      this.errorMessage.set('Network error. Backend API may be offline.');
    }
  }

  logout() {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('verifier_email');
    this.isAuthenticated.set(false);
    this.email = '';
    this.password = '';
    this.errorMessage.set('');
  }
}
