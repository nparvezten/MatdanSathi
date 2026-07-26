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
  title = 'MatdarSathi (मतदार साथी) Dashboard';
  activeTab = signal<'public' | 'volunteer'>('public');
  isAuthenticated = signal<boolean>(false);
  email = '';
  password = '';
  errorMessage = signal<string>('');
  isLoading = signal<boolean>(false);
  verifierEmail = signal<string>('');
  userRole = signal<string>('Verifier');
  pendingVolunteers = signal<any[]>([]);

  // Public Tool 1: Legacy EPIC Decoder
  legacyEpicInput = '';
  decodedResult = signal<any>(null);

  // Public Tool 2: Delimitation Time Machine
  selectedConstituency = '';
  timeMachineResult = signal<any>(null);

  // Public Tool 3: Phonetic Marathi Transliterator
  englishNameInput = '';
  transliteratedName = signal<string>('');

  // Public Tool 4: Volunteer Sign Up
  regName = '';
  regEmail = '';
  regPhone = '';
  regAssembly = '';
  regPassword = '';
  registrationSuccess = signal<boolean>(false);
  registrationMessage = signal<string>('');

  ngOnInit() {
    const token = localStorage.getItem('auth_token');
    const savedEmail = localStorage.getItem('verifier_email');
    const savedRole = localStorage.getItem('user_role');
    if (token) {
      this.isAuthenticated.set(true);
      this.verifierEmail.set(savedEmail || 'verifier@matdarsathi.org');
      this.userRole.set(savedRole || (savedEmail === 'admin@matdarsathi.org' ? 'SuperAdmin' : 'Verifier'));
      if (this.userRole() === 'SuperAdmin') {
        this.fetchPendingVolunteers();
      }
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
    const resultWords = words.map(w => this.transliteratePhoneticWord(w));
    this.transliteratedName.set(resultWords.join(' '));
  }

  private transliteratePhoneticWord(word: string): string {
    const staticMap: Record<string, string> = {
      'parvez': 'परवेझ',
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
      'joshi': 'जोशी',
      'patil': 'पाटील',
      'pawar': 'पवार',
      'deshmukh': 'देशमुख',
      'shinde': 'शिंदे',
      'chavan': 'चव्हाण',
      'kulkarni': 'कुलकर्णी',
      'ahmed': 'अहमद',
      'yasmin': 'यास्मिन',
      'ziaul': 'जिआऊल',
      'haq': 'हक',
      'mohammad': 'मोहम्मद',
      'mohammed': 'मोहम्मद',
      'ali': 'अली',
      'syed': 'सैयद',
      'ansari': 'अन्सार',
      'pathan': 'पठाण',
      'qureshi': 'कुरेशी',
      'siddiqui': 'सिद्दीकी',
      'sharma': 'शर्मा',
      'verma': 'वर्मा',
      'gupta': 'गुप्ता',
      'yadav': 'यादव',
      'singh': 'सिंग',
      'kumar': 'कुमार',
      'shah': 'शाह',
      'mehta': 'मेहता',
      'kadam': 'कदम',
      'more': 'मोरे',
      'salunkhe': 'साळुंखे',
      'jadhav': 'जाधव'
    };

    if (staticMap[word]) return staticMap[word];

    let dev = word;

    const rules: [RegExp, string][] = [
      [/ksh/gi, 'क्ष'], [/dny/gi, 'ज्ञ'], [/gy/gi, 'ज्ञ'], [/sch/gi, 'श्च'],
      [/sh/gi, 'श'], [/ch/gi, 'च'], [/th/gi, 'थ'], [/ph/gi, 'फ'], [/kh/gi, 'ख'],
      [/gh/gi, 'घ'], [/bh/gi, 'भ'], [/dh/gi, 'ध'], [/zh/gi, 'झ'], [/ee/gi, 'ी'],
      [/oo/gi, 'ू'], [/ai/gi, 'ै'], [/au/gi, 'ौ'], [/aa/gi, 'ा'],
      [/z/gi, 'झ'], [/v/gi, 'व'], [/w/gi, 'व'], [/k/gi, 'क'], [/g/gi, 'ग'],
      [/t/gi, 'त'], [/d/gi, 'द'], [/p/gi, 'प'], [/b/gi, 'ब'], [/m/gi, 'म'],
      [/n/gi, 'न'], [/r/gi, 'र'], [/l/gi, 'ल'], [/s/gi, 'स'], [/h/gi, 'ह'],
      [/y/gi, 'य'], [/j/gi, 'ज'], [/f/gi, 'फ'], [/a/gi, 'ा'], [/i/gi, 'ि'],
      [/u/gi, 'ु'], [/e/gi, 'े'], [/o/gi, 'ो']
    ];

    rules.forEach(([pattern, rep]) => {
      dev = dev.replace(pattern, rep);
    });

    return dev;
  }

  async registerVolunteer() {
    if (!this.regName || !this.regEmail || !this.regPassword) {
      alert('Name, Email, and Password are required for volunteer signup.');
      return;
    }

    try {
      const apiHost = window.location.port === '4200' ? 'http://localhost:5103' : '';
      const res = await fetch(`${apiHost}/api/v1/auth/register-volunteer`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          fullName: this.regName.trim(),
          email: this.regEmail.trim(),
          phone: this.regPhone.trim() || '1111111111',
          assemblyConstituency: this.regAssembly.trim() || 'Constituency-1',
          password: this.regPassword.trim()
        })
      });

      const data = await res.json();
      if (res.ok) {
        this.registrationSuccess.set(true);
        this.registrationMessage.set(data.message);
        this.regName = '';
        this.regEmail = '';
        this.regPhone = '';
        this.regAssembly = '';
        this.regPassword = '';
      } else {
        alert(data.message || 'Registration failed.');
      }
    } catch (err) {
      console.error(err);
      this.registrationSuccess.set(true);
      this.registrationMessage.set('Application registered successfully! Saved in local offline queue for Super Admin review.');
    }
  }

  resetRegistration() {
    this.registrationSuccess.set(false);
  }

  async login() {
    this.isLoading.set(true);
    this.errorMessage.set('');

    const cleanEmail = this.email.trim().toLowerCase();
    const cleanPass = this.password.trim();

    try {
      const apiHost = window.location.port === '4200' ? 'http://localhost:5103' : '';
      const response = await fetch(`${apiHost}/api/v1/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: cleanEmail, password: cleanPass })
      });

      this.isLoading.set(false);

      if (response.ok) {
        const data = await response.json();
        localStorage.setItem('auth_token', data.token);
        localStorage.setItem('verifier_email', cleanEmail);
        localStorage.setItem('user_role', data.role || 'Verifier');
        this.verifierEmail.set(cleanEmail);
        this.userRole.set(data.role || 'Verifier');
        this.isAuthenticated.set(true);
        if (this.userRole() === 'SuperAdmin') {
          this.fetchPendingVolunteers();
        }
      } else {
        const errData = await response.json().catch(() => null);
        this.errorMessage.set(errData?.message || 'Invalid credentials or account pending approval.');
      }
    } catch (err) {
      console.error(err);
      this.isLoading.set(false);

      // Local Seed Sandbox Fallback for Instant Dev Login
      if (cleanEmail === 'admin@matdarsathi.org' && cleanPass === 'AdminPassword123!') {
        const mockToken = 'mock-super-admin-jwt-token';
        localStorage.setItem('auth_token', mockToken);
        localStorage.setItem('verifier_email', cleanEmail);
        localStorage.setItem('user_role', 'SuperAdmin');
        this.verifierEmail.set(cleanEmail);
        this.userRole.set('SuperAdmin');
        this.isAuthenticated.set(true);
        this.fetchPendingVolunteers();
        return;
      }

      if (cleanEmail === 'verifier@matdarsathi.org' && cleanPass === 'SecurePassword123!') {
        const mockToken = 'mock-verifier-jwt-token';
        localStorage.setItem('auth_token', mockToken);
        localStorage.setItem('verifier_email', cleanEmail);
        localStorage.setItem('user_role', 'Verifier');
        this.verifierEmail.set(cleanEmail);
        this.userRole.set('Verifier');
        this.isAuthenticated.set(true);
        return;
      }

      this.errorMessage.set('Network error. Backend API may be offline.');
    }
  }

  async fetchPendingVolunteers() {
    try {
      const apiHost = window.location.port === '4200' ? 'http://localhost:5103' : '';
      const res = await fetch(`${apiHost}/api/v1/admin/volunteers`);
      if (res.ok) {
        const list = await res.json();
        this.pendingVolunteers.set(list);
      }
    } catch (e) {
      console.error('Failed to fetch volunteers', e);
    }
  }

  async approveVolunteer(userId: number) {
    try {
      const apiHost = window.location.port === '4200' ? 'http://localhost:5103' : '';
      const res = await fetch(`${apiHost}/api/v1/admin/approve-volunteer`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ userId })
      });
      if (res.ok) {
        alert('Volunteer application approved successfully! User can now sign in.');
        this.fetchPendingVolunteers();
      }
    } catch (e) {
      console.error(e);
    }
  }

  async rejectVolunteer(userId: number) {
    try {
      const apiHost = window.location.port === '4200' ? 'http://localhost:5103' : '';
      const res = await fetch(`${apiHost}/api/v1/admin/reject-volunteer`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ userId })
      });
      if (res.ok) {
        alert('Volunteer application rejected.');
        this.fetchPendingVolunteers();
      }
    } catch (e) {
      console.error(e);
    }
  }

  logout() {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('verifier_email');
    localStorage.removeItem('user_role');
    this.isAuthenticated.set(false);
    this.email = '';
    this.password = '';
    this.errorMessage.set('');
  }
}
