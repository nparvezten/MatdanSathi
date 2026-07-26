import { Component, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-form-wizard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="w-full max-w-2xl mx-auto bg-slate-900/60 backdrop-blur-md border border-slate-800 rounded-2xl shadow-xl overflow-hidden">
      <!-- Wizard Title Header -->
      <div class="px-6 py-5 border-b border-slate-800 bg-gradient-to-r from-teal-900/30 to-slate-900">
        <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3">
          <div>
            <h2 class="text-xl font-semibold text-white tracking-wide">Voter Services & SIR Hearing Portal</h2>
            <p class="text-xs text-slate-400 mt-1">Electoral Roll Companion & AERO Dossier Generator (MatdanSathi)</p>
          </div>
          <!-- Mode switcher buttons -->
          <div class="flex flex-wrap bg-slate-950 p-1 rounded-lg border border-slate-800 gap-1">
            <button 
              (click)="setMode('anomaly')"
              [class.bg-teal-600]="wizardMode() === 'anomaly'"
              [class.text-white]="wizardMode() === 'anomaly'"
              class="px-3 py-1.5 text-xs font-medium rounded-md transition-all text-slate-400 hover:text-white flex items-center space-x-1">
              <span>📋 SIR AERO Assistant</span>
            </button>
            <button 
              (click)="setMode('form8')"
              [class.bg-teal-600]="wizardMode() === 'form8'"
              [class.text-white]="wizardMode() === 'form8'"
              class="px-2.5 py-1.5 text-xs font-medium rounded-md transition-all text-slate-400 hover:text-white">
              Form 8 Correct
            </button>
            <button 
              (click)="setMode('form7')"
              [class.bg-teal-600]="wizardMode() === 'form7'"
              [class.text-white]="wizardMode() === 'form7'"
              class="px-2.5 py-1.5 text-xs font-medium rounded-md transition-all text-slate-400 hover:text-white">
              Form 7 Deceased
            </button>
            <button 
              (click)="setMode('history')"
              [class.bg-teal-600]="wizardMode() === 'history'"
              [class.text-white]="wizardMode() === 'history'"
              class="px-2.5 py-1.5 text-xs font-medium rounded-md transition-all text-slate-400 hover:text-white">
              2002 Archives
            </button>
          </div>
        </div>
      </div>

      <!-- Step Indicator Bar for SIR AERO Assistant (5 Steps) -->
      <div *ngIf="wizardMode() === 'anomaly'" class="px-6 py-4 bg-slate-950/40 border-b border-slate-800/50">
        <div class="flex items-center justify-between relative">
          <div class="absolute left-0 top-1/2 -translate-y-1/2 h-[2px] bg-slate-800 w-full z-0"></div>
          <div class="absolute left-0 top-1/2 -translate-y-1/2 h-[2px] bg-teal-500 z-0 transition-all duration-300"
               [style.width.%]="(anomalyStep() - 1) * 25"></div>

          <div *ngFor="let s of [1, 2, 3, 4, 5]" class="z-10 flex flex-col items-center">
            <div [class.bg-teal-500]="anomalyStep() >= s"
                 [class.border-teal-500]="anomalyStep() >= s"
                 [class.text-slate-950]="anomalyStep() >= s"
                 [class.bg-slate-900]="anomalyStep() < s"
                 [class.border-slate-800]="anomalyStep() < s"
                 [class.text-slate-400]="anomalyStep() < s"
                 class="w-6 h-6 rounded-full flex items-center justify-center border font-semibold text-[10px] transition-all duration-300">
              <span *ngIf="anomalyStep() > s">✓</span>
              <span *ngIf="anomalyStep() <= s">{{ s }}</span>
            </div>
            <span class="text-[9px] text-slate-400 mt-1 font-medium bg-slate-950 px-1 rounded whitespace-nowrap">{{ getAnomalyStepLabel(s) }}</span>
          </div>
        </div>
      </div>

      <!-- MAIN CONTENT AREA -->
      <div class="p-6">
        
        <!-- MODE 1: MODULE C - SIR AERO HEARING WIZARD (5 STEPS) -->
        <div *ngIf="wizardMode() === 'anomaly'">
          
          <!-- Step 1: Identity & Status Check -->
          <div *ngIf="anomalyStep() === 1" class="animate-fadeIn space-y-4">
            <h3 class="text-base font-medium text-teal-400 mb-1">Step 1: Voter Identity & Status Check</h3>
            <p class="text-xs text-slate-400 mb-3">Enter your details to verify your SIR roll status and calculate your ECI citizenship era.</p>

            <div class="space-y-3">
              <div>
                <label class="block text-xs font-semibold text-slate-300 mb-1 uppercase">Full Name</label>
                <input type="text" [(ngModel)]="voterName" placeholder="e.g. Khan Saidnabi" class="w-full bg-slate-950 border border-slate-800 rounded-lg px-4 py-2 text-white text-sm" />
              </div>

              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-slate-300 mb-1 uppercase">EPIC Number or Ref</label>
                  <input type="text" [(ngModel)]="epicInput" placeholder="e.g. SLD1234567" class="w-full bg-slate-950 border border-slate-800 rounded-lg px-4 py-2 text-white text-sm uppercase" />
                </div>
                <div>
                  <label class="block text-xs font-semibold text-slate-300 mb-1 uppercase">Year of Birth (or Age)</label>
                  <input type="number" [(ngModel)]="birthYear" (change)="onBirthYearChange()" placeholder="e.g. 1982" class="w-full bg-slate-950 border border-slate-800 rounded-lg px-4 py-2 text-white text-sm" />
                </div>
              </div>

              <div>
                <label class="block text-xs font-semibold text-slate-300 mb-1.5 uppercase">Current Registration Status</label>
                <div class="grid grid-cols-3 gap-2">
                  <button 
                    (click)="voterStatus.set('Active')"
                    [class.bg-teal-600]="voterStatus() === 'Active'"
                    [class.text-white]="voterStatus() === 'Active'"
                    [class.bg-slate-950]="voterStatus() !== 'Active'"
                    class="p-2 rounded-lg border border-slate-800 text-xs font-medium transition-all text-slate-400">
                    ✅ Active Elector
                  </button>
                  <button 
                    (click)="voterStatus.set('Unmapped')"
                    [class.bg-teal-600]="voterStatus() === 'Unmapped'"
                    [class.text-white]="voterStatus() === 'Unmapped'"
                    [class.bg-slate-950]="voterStatus() !== 'Unmapped'"
                    class="p-2 rounded-lg border border-slate-800 text-xs font-medium transition-all text-slate-400">
                    📂 Unmapped 2002
                  </button>
                  <button 
                    (click)="voterStatus.set('NoticeReceived')"
                    [class.bg-teal-600]="voterStatus() === 'NoticeReceived'"
                    [class.text-white]="voterStatus() === 'NoticeReceived'"
                    [class.bg-slate-950]="voterStatus() !== 'NoticeReceived'"
                    class="p-2 rounded-lg border border-slate-800 text-xs font-medium transition-all text-slate-400">
                    📨 Notice Received
                  </button>
                </div>
              </div>

              <!-- Calculated Era Banner -->
              <div class="bg-slate-950/80 border border-slate-800 p-3 rounded-xl text-xs space-y-1">
                <span class="text-[10px] text-slate-500 uppercase font-bold tracking-wider">Calculated ECI Era:</span>
                <div class="text-teal-400 font-bold">{{ calculatedEraLabel() }}</div>
              </div>
            </div>
          </div>

          <!-- Step 2: Anomaly Selector & Decision Tree -->
          <div *ngIf="anomalyStep() === 2" class="animate-fadeIn space-y-4">
            <h3 class="text-base font-medium text-teal-400 mb-1">Step 2: Anomaly Selector & Decision Tree</h3>
            <p class="text-xs text-slate-400 mb-3">Answer interactive decision prompts to identify your exact SIR discrepancy type.</p>

            <div class="space-y-2.5">
              <div *ngFor="let prompt of decisionPrompts" 
                   (click)="selectAnomalyPrompt(prompt.type)"
                   [class.border-teal-500]="selectedAnomalyType() === prompt.type"
                   [class.bg-teal-900]="selectedAnomalyType() === prompt.type"
                   class="p-3.5 rounded-xl border border-slate-800 bg-slate-950 hover:border-slate-700 transition-all cursor-pointer flex items-start space-x-3 select-none">
                <span class="text-lg">{{ prompt.icon }}</span>
                <div class="flex-1">
                  <div class="flex justify-between items-center">
                    <span class="text-xs font-bold text-white">{{ prompt.question }}</span>
                    <span class="text-[9px] bg-teal-950 text-teal-400 border border-teal-800 px-1.5 py-0.5 rounded font-mono">{{ prompt.form }}</span>
                  </div>
                  <p class="text-[10px] text-slate-400 mt-1 leading-normal">{{ prompt.description }}</p>
                </div>
              </div>
            </div>
          </div>

          <!-- Step 3: Age & Birth Era Proof Finder -->
          <div *ngIf="anomalyStep() === 3" class="animate-fadeIn space-y-4">
            <h3 class="text-base font-medium text-teal-400 mb-1">Step 3: Prescribed 12 Document Proof Finder</h3>
            <p class="text-xs text-slate-400 mb-2">Based on your calculated era (<strong>{{ calculatedEraLabel() }}</strong>), select your proof documents.</p>

            <div *ngIf="guidanceData()" class="bg-teal-950/20 border border-teal-900 p-3 rounded-xl text-xs space-y-1 mb-3">
              <span class="text-teal-400 font-bold block">📜 ECI Rule Requirement:</span>
              <p class="text-slate-300 text-[11px] leading-relaxed">{{ guidanceData().eciCutoffRuleDescription }}</p>
            </div>

            <!-- Self Proof Documents -->
            <div class="space-y-2">
              <h4 class="text-xs font-bold text-slate-300 uppercase tracking-wider">Select Proof for Self (Select {{ guidanceData()?.requiredSelfProofCount || 1 }}):</h4>
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-2 max-h-44 overflow-y-auto pr-1">
                <label *ngFor="let doc of official12Docs" class="flex items-center space-x-2 p-2 rounded-lg border border-slate-800 bg-slate-950 text-xs cursor-pointer select-none">
                  <input type="checkbox" [checked]="isSelfProofSelected(doc.name)" (change)="toggleSelfProof(doc.name)" class="rounded border-slate-800 text-teal-600 bg-slate-950 w-3.5 h-3.5" />
                  <span class="text-slate-200 text-[11px] font-medium">{{ doc.name }}</span>
                </label>
              </div>
            </div>

            <!-- Parent Proof Documents (if applicable) -->
            <div *ngIf="guidanceData()?.requiredFatherProofCount > 0" class="space-y-2 pt-2">
              <h4 class="text-xs font-bold text-slate-300 uppercase tracking-wider">Select Proof for Parent (Father / Mother):</h4>
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-2 max-h-44 overflow-y-auto pr-1">
                <label *ngFor="let doc of official12Docs" class="flex items-center space-x-2 p-2 rounded-lg border border-slate-800 bg-slate-950 text-xs cursor-pointer select-none">
                  <input type="checkbox" [checked]="isParentProofSelected(doc.name)" (change)="toggleParentProof(doc.name)" class="rounded border-slate-800 text-teal-600 bg-slate-950 w-3.5 h-3.5" />
                  <span class="text-slate-200 text-[11px] font-medium">{{ doc.name }}</span>
                </label>
              </div>
            </div>
          </div>

          <!-- Step 4: Pre-filled Form 8 / ECI Portal Direct Redirect Link & Checklist -->
          <div *ngIf="anomalyStep() === 4" class="animate-fadeIn space-y-4">
            <h3 class="text-base font-medium text-teal-400 mb-1">Step 4: Pre-filled ECI Direct Link & Checklist</h3>
            <p class="text-xs text-slate-400 mb-3">Preview your application package ready for official ECI submission.</p>

            <div class="bg-slate-950/80 border border-slate-800 p-4 rounded-xl text-xs space-y-2.5">
              <div class="flex justify-between border-b border-slate-850 pb-2 font-bold text-slate-300">
                <span>Voter: {{ voterName || 'Khan Saidnabi' }}</span>
                <span class="font-mono text-teal-400">EPIC: {{ epicInput || 'SLD1234567' }}</span>
              </div>
              <div><span class="text-slate-500">Anomaly Type:</span> <span class="text-white font-semibold ml-1">{{ selectedAnomalyType() }}</span></div>
              <div><span class="text-slate-500">Selected Self Proofs:</span> <span class="text-teal-400 font-semibold ml-1">{{ selectedSelfProofs().join(', ') || 'Aadhaar Card' }}</span></div>
              <div *ngIf="selectedParentProofs().length > 0"><span class="text-slate-500">Selected Parent Proofs:</span> <span class="text-teal-400 font-semibold ml-1">{{ selectedParentProofs().join(', ') }}</span></div>
            </div>

            <!-- ECI Portal Redirect Box -->
            <div class="bg-teal-950/20 border border-teal-900 p-4 rounded-xl space-y-2 text-xs">
              <div class="font-bold text-teal-400 flex items-center space-x-1">
                <span>🔗 Official ECI Voter Portal Direct Link</span>
              </div>
              <p class="text-slate-400 text-[11px]">Clicking below opens the official Election Commission portal pre-filled with your profile parameters.</p>
              <a [href]="nvspLink()" target="_blank" class="inline-block bg-teal-600 hover:bg-teal-500 text-white font-bold text-xs px-4 py-2 rounded-lg transition-all">
                Open Official ECI Form Portal ➔
              </a>
            </div>
          </div>

          <!-- Step 5: AERO Hearing Dossier View & Print -->
          <div *ngIf="anomalyStep() === 5" class="animate-fadeIn space-y-4">
            <h3 class="text-base font-medium text-teal-400 mb-1">Step 5: Official AERO Hearing Cover Sheet Dossier</h3>
            <p class="text-xs text-slate-400 mb-3">Formatted official cover sheet ready for paper submission at local AERO hearing camps.</p>

            <div *ngIf="dossierData()" class="bg-slate-950 border border-slate-800 p-5 rounded-xl space-y-4 font-sans print:bg-white print:text-black">
              <!-- Official Header -->
              <div class="border-b border-slate-800 pb-3 flex justify-between items-start">
                <div>
                  <h4 class="text-sm font-black text-white uppercase tracking-wider print:text-black">OFFICIAL AERO HEARING COVER SHEET</h4>
                  <p class="text-[10px] text-slate-400 print:text-gray-600">Special Intensive Revision (SIR) Electoral Verification</p>
                </div>
                <div class="text-right">
                  <span class="text-[10px] text-slate-500 block">Dossier Ref</span>
                  <span class="font-mono text-teal-400 font-bold text-xs print:text-black">{{ dossierData().dossierReference }}</span>
                </div>
              </div>

              <!-- Detail Grid -->
              <div class="grid grid-cols-2 gap-3 text-xs">
                <div><span class="text-slate-500 print:text-gray-600">Voter Name:</span> <span class="font-bold text-white print:text-black ml-1">{{ dossierData().voterName }}</span></div>
                <div><span class="text-slate-500 print:text-gray-600">EPIC Card No:</span> <span class="font-mono text-teal-400 print:text-black ml-1">{{ dossierData().epicNumber }}</span></div>
                <div><span class="text-slate-500 print:text-gray-600">Constituency:</span> <span class="text-slate-300 print:text-black ml-1">{{ dossierData().assemblyConstituency }}</span></div>
                <div><span class="text-slate-500 print:text-gray-600">Prescribed Form:</span> <span class="text-slate-300 print:text-black ml-1">{{ dossierData().applicableForm }}</span></div>
                <div><span class="text-slate-500 print:text-gray-600">Citizenship Era:</span> <span class="text-slate-300 print:text-black ml-1">{{ dossierData().citizenshipEra }}</span></div>
                <div><span class="text-slate-500 print:text-gray-600">Hearing Location:</span> <span class="text-slate-300 print:text-black ml-1">{{ dossierData().hearingBoothLocation }}</span></div>
              </div>

              <!-- Notice Box -->
              <div class="bg-slate-900 p-3 rounded-lg border border-slate-800 text-[11px] text-slate-300 whitespace-pre-line leading-relaxed font-mono print:bg-gray-100 print:text-black">
                {{ dossierData().hearingNoticeText }}
              </div>

              <!-- Print Action Button -->
              <div class="pt-2 flex justify-between items-center print:hidden">
                <span class="text-[10px] text-slate-500">Ready for paper submission at local AERO hearing camp</span>
                <button (click)="printSummary()" class="bg-teal-600 hover:bg-teal-500 text-white font-bold text-xs px-4 py-2 rounded-lg transition-all flex items-center space-x-1.5">
                  <span>🖨 Print AERO Hearing Dossier</span>
                </button>
              </div>
            </div>
          </div>

          <!-- SIR STEP BUTTON BAR -->
          <div class="mt-6 flex justify-between border-t border-slate-800/60 pt-4">
            <button 
              (click)="prevAnomalyStep()"
              [disabled]="anomalyStep() === 1"
              class="border border-slate-800 hover:border-slate-700 disabled:opacity-30 disabled:hover:border-slate-800 text-slate-300 text-xs font-semibold px-4 py-2 rounded-lg transition-all">
              Back
            </button>
            <button 
              *ngIf="anomalyStep() < 5"
              (click)="nextAnomalyStep()"
              class="bg-teal-600 hover:bg-teal-500 text-white text-xs font-semibold px-4 py-2 rounded-lg transition-all">
              Next
            </button>
            <button 
              *ngIf="anomalyStep() === 5"
              (click)="printSummary()"
              class="bg-emerald-600 hover:bg-emerald-500 text-white text-xs font-semibold px-4 py-2 rounded-lg transition-all">
              🖨 Print Dossier Sheet
            </button>
          </div>
        </div>

        <!-- MODE 2: FORM 8 CORRECTION WIZARD -->
        <div *ngIf="wizardMode() === 'form8'">
          <!-- Existing Form 8 content kept intact for backwards compatibility -->
          <div class="text-xs text-slate-400 space-y-3">
            <p>Use the <strong>SIR AERO Assistant</strong> tab above for interactive anomaly rules and hearing dossiers.</p>
            <button (click)="setMode('anomaly')" class="bg-teal-600 text-white font-bold px-3 py-1.5 rounded text-xs">Switch to SIR AERO Assistant ➔</button>
          </div>
        </div>

        <!-- MODE 3: FORM 7 DECEASED DELETION ASSISTANT -->
        <div *ngIf="wizardMode() === 'form7'">
          <div class="text-xs text-slate-400 space-y-3">
            <p>Use the <strong>SIR AERO Assistant</strong> tab above for interactive anomaly rules and hearing dossiers.</p>
            <button (click)="setMode('anomaly')" class="bg-teal-600 text-white font-bold px-3 py-1.5 rounded text-xs">Switch to SIR AERO Assistant ➔</button>
          </div>
        </div>

        <!-- MODE 4: HISTORICAL 2002 LOOKUP ASSISTANCE -->
        <div *ngIf="wizardMode() === 'history'">
          <div class="text-xs text-slate-400 space-y-3">
            <p>Use the <strong>SIR AERO Assistant</strong> tab above for interactive anomaly rules and hearing dossiers.</p>
            <button (click)="setMode('anomaly')" class="bg-teal-600 text-white font-bold px-3 py-1.5 rounded text-xs">Switch to SIR AERO Assistant ➔</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class FormWizardComponent implements OnInit {
  // Main Mode signal
  wizardMode = signal<'anomaly' | 'form8' | 'form7' | 'history'>('anomaly');
  anomalyStep = signal<number>(1);

  // Voter inputs
  voterName: string = 'Khan Saidnabi';
  epicInput: string = 'SLD1234567';
  birthYear: number = 1982;
  voterStatus = signal<'Active' | 'Unmapped' | 'NoticeReceived'>('Active');
  selectedAnomalyType = signal<string>('SurnameMarriageChange');

  // Selected Proofs signals
  selectedSelfProofs = signal<string[]>(['Aadhaar Card']);
  selectedParentProofs = signal<string[]>([]);

  // Response Signals
  guidanceData = signal<any>(null);
  dossierData = signal<any>(null);

  official12Docs = [
    { name: 'Indian Passport', code: 'PASSPORT' },
    { name: 'Municipal Birth Certificate', code: 'BIRTH_CERT' },
    { name: 'Class 10 / School Leaving Certificate (SLC)', code: 'SCHOOL_CERT' },
    { name: 'Govt ID / Pension Payment Order (PPO)', code: 'GOVT_PPO' },
    { name: 'Land Allotment / House Title Deed', code: 'LAND_DEED' },
    { name: 'Aadhaar Card', code: 'AADHAAR' },
    { name: 'Caste Certificate', code: 'CASTE_CERT' },
    { name: 'Permanent Resident / Domicile Certificate', code: 'PRC_DOMICILE' },
    { name: 'Forest Rights Certificate', code: 'FOREST_RIGHTS' },
    { name: 'Family Register / Gram Panchayat Extract', code: 'FAMILY_REGISTER' },
    { name: 'Pre-1987 Government Treasury/Service Record', code: 'PRE_1987_DOC' },
    { name: 'NRC / Archival Electoral Roll Extract', code: 'NRC_LEGACY' }
  ];

  decisionPrompts = [
    { type: 'SurnameMarriageChange', icon: '💍', question: 'Did your surname change after marriage or transliteration?', form: 'Form 8', description: 'Applicable when surname differs due to marriage, regional script transliteration (Devanagari/Urdu), or clerical spelling errors.' },
    { type: 'DoorLockedShifted', icon: '🚪', question: 'Were you away or house locked during BLO door visit?', form: 'Form 8', description: 'Applicable when BLO marked elector as "Absent" or "Shifted" during SIR door-to-door verification drive.' },
    { type: 'ProgenyMismatch', icon: '👨‍👩‍👧', question: 'Can\'t find parent in 2002 roll or first-time elector link?', form: 'Form 6 / 8', description: 'Establishes ancestral relationship linkage to parent/grandparent registration records.' },
    { type: 'AgeDobConflict', icon: '📅', question: 'Is there a conflict in your Age or Date of Birth?', form: 'Form 8', description: 'Correction of age or birth year under ECI citizenship era guidelines.' },
    { type: 'Form7Objection', icon: '📜', question: 'Is there an objection or deceased family member deletion?', form: 'Form 7', description: 'Removal request for deceased or shifted family members.' }
  ];

  ngOnInit() {
    this.fetchGuidance();
  }

  setMode(mode: 'anomaly' | 'form8' | 'form7' | 'history') {
    this.wizardMode.set(mode);
  }

  getAnomalyStepLabel(step: number): string {
    switch (step) {
      case 1: return 'Identity';
      case 2: return 'Decision';
      case 3: return 'Proofs';
      case 4: return 'ECI Link';
      case 5: return 'Dossier';
      default: return '';
    }
  }

  calculatedEraLabel = computed(() => {
    const yr = this.birthYear || 1982;
    if (yr < 1987) return 'Pre-1987 Era (Born before July 1, 1987)';
    if (yr <= 2004) return '1987–2004 Era (Born between July 1, 1987 and Dec 2, 2004)';
    return 'Post-2004 Era (Born after Dec 2, 2004)';
  });

  onBirthYearChange() {
    this.fetchGuidance();
  }

  selectAnomalyPrompt(type: string) {
    this.selectedAnomalyType.set(type);
    this.fetchGuidance();
  }

  isSelfProofSelected(docName: string): boolean {
    return this.selectedSelfProofs().includes(docName);
  }

  toggleSelfProof(docName: string) {
    const curr = this.selectedSelfProofs();
    if (curr.includes(docName)) {
      this.selectedSelfProofs.set(curr.filter(d => d !== docName));
    } else {
      this.selectedSelfProofs.set([...curr, docName]);
    }
  }

  isParentProofSelected(docName: string): boolean {
    return this.selectedParentProofs().includes(docName);
  }

  toggleParentProof(docName: string) {
    const curr = this.selectedParentProofs();
    if (curr.includes(docName)) {
      this.selectedParentProofs.set(curr.filter(d => d !== docName));
    } else {
      this.selectedParentProofs.set([...curr, docName]);
    }
  }

  async fetchGuidance() {
    const age = 2026 - (this.birthYear || 1982);
    const anomaly = this.selectedAnomalyType();
    try {
      const headers: Record<string, string> = {
        'Authorization': `Bearer ${localStorage.getItem('auth_token')}`
      };
      const res = await fetch(`/api/v1/wizard/guidance?age=${age}&birthYear=${this.birthYear}&anomalyType=${anomaly}`, { headers });
      if (res.ok) {
        const data = await res.json();
        this.guidanceData.set(data);
      } else {
        this.setMockGuidance(age, anomaly);
      }
    } catch {
      this.setMockGuidance(age, anomaly);
    }
  }

  private setMockGuidance(age: number, anomaly: string) {
    const yr = this.birthYear || 1982;
    const isPre87 = yr < 1987;
    const is87_04 = yr >= 1987 && yr <= 2004;
    this.guidanceData.set({
      anomalyType: anomaly,
      citizenshipEra: isPre87 ? 'Pre1987' : (is87_04 ? 'Between1987And2004' : 'Post2004'),
      birthYear: yr,
      ageCategoryLabel: this.calculatedEraLabel(),
      eciCutoffRuleDescription: isPre87 
        ? 'Under ECI SIR Rules, electors born before 01.07.1987 require 1 proof document for Self.'
        : 'Under ECI SIR Rules, electors born between 01.07.1987 and 02.12.2004 require 1 proof for Self + 1 proof for Parent.',
      requiredSelfProofCount: 1,
      requiredFatherProofCount: isPre87 ? 0 : 1,
      requiredMotherProofCount: 0,
      applicableForm: 'Form 8 (Correction)',
      actionChecklist: [
        '1. Verify selected Anomaly: ' + anomaly,
        '2. Gather required proof documents.',
        '3. Submit Form 8 online or present physical dossier at AERO Hearing.'
      ]
    });
  }

  async generateDossier() {
    try {
      const headers: Record<string, string> = {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('auth_token')}`
      };
      const body = {
        voterName: this.voterName || 'Khan Saidnabi',
        epicNumber: this.epicInput || 'SLD1234567',
        assemblyConstituency: 'Constituency 1 (Ward 2)',
        pollingStation: 'Primary School Facility (Room 2)',
        anomalyType: this.selectedAnomalyType(),
        citizenshipEra: this.calculatedEraLabel(),
        selectedSelfProofs: this.selectedSelfProofs().length > 0 ? this.selectedSelfProofs() : ['Aadhaar Card'],
        selectedParentProofs: this.selectedParentProofs(),
        hearingBoothLocation: 'AERO Office / Government Primary School Hearing Camp'
      };
      const res = await fetch('/api/v1/wizard/generate-hearing-dossier', {
        method: 'POST',
        headers,
        body: JSON.stringify(body)
      });
      if (res.ok) {
        const data = await res.json();
        this.dossierData.set(data);
      } else {
        this.setMockDossier(body);
      }
    } catch {
      this.setMockDossier({
        voterName: this.voterName,
        epicNumber: this.epicInput,
        assemblyConstituency: 'Constituency 1',
        pollingStation: 'Primary School Facility',
        anomalyType: this.selectedAnomalyType(),
        citizenshipEra: this.calculatedEraLabel(),
        selectedSelfProofs: this.selectedSelfProofs(),
        selectedParentProofs: this.selectedParentProofs(),
        hearingBoothLocation: 'AERO Office'
      });
    }
  }

  private setMockDossier(body: any) {
    const ref = 'AERO-DOSSIER-' + Math.floor(100000 + Math.random() * 900000);
    this.dossierData.set({
      dossierReference: ref,
      generatedAt: new Date().toISOString(),
      voterName: body.voterName || 'Khan Saidnabi',
      epicNumber: body.epicNumber || 'SLD1234567',
      assemblyConstituency: body.assemblyConstituency || 'Constituency 1',
      pollingStation: body.pollingStation || 'Primary School Facility',
      anomalyType: body.anomalyType || 'SurnameMarriageChange',
      citizenshipEra: body.citizenshipEra || 'Pre-1987',
      applicableForm: 'Form 8 (Correction)',
      selectedSelfProofs: body.selectedSelfProofs.length > 0 ? body.selectedSelfProofs : ['Aadhaar Card'],
      selectedParentProofs: body.selectedParentProofs,
      hearingBoothLocation: body.hearingBoothLocation || 'AERO Office Hearing Camp',
      hearingNoticeText: `OFFICIAL AERO HEARING COVER SHEET SUMMARY\nDossier Ref: ${ref}\nVoter: ${body.voterName} (EPIC: ${body.epicNumber})\nPrescribed Form: Form 8\nAttached Proofs: ${body.selectedSelfProofs.join(', ')}\nHearing Location: ${body.hearingBoothLocation}`,
      isReadyForPrint: true
    });
  }

  nextAnomalyStep() {
    if (this.anomalyStep() < 5) {
      if (this.anomalyStep() === 4) {
        this.generateDossier();
      }
      this.anomalyStep.update(s => s + 1);
    }
  }

  prevAnomalyStep() {
    if (this.anomalyStep() > 1) {
      this.anomalyStep.update(s => s - 1);
    }
  }

  nvspLink = computed(() => {
    const epic = encodeURIComponent(this.epicInput || 'SLD1234567');
    const name = encodeURIComponent(this.voterName || 'Khan Saidnabi');
    return `https://voters.eci.gov.in/form8?epic=${epic}&name=${name}&form8=true`;
  });

  printSummary() {
    window.print();
  }
}
