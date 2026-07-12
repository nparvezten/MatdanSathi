import { Component, signal, computed } from '@angular/core';
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
        <div class="flex items-center justify-between">
          <div>
            <h2 class="text-xl font-semibold text-white tracking-wide">Voter Services Portal</h2>
            <p class="text-xs text-slate-400 mt-1">Electoral Roll Companion (MatdanSathi)</p>
          </div>
          <!-- Mode switcher -->
          <div class="flex bg-slate-950 p-1 rounded-lg border border-slate-800">
            <button 
              (click)="setMode('form8')"
              [class.bg-teal-600]="wizardMode() === 'form8'"
              [class.text-white]="wizardMode() === 'form8'"
              class="px-3 py-1.5 text-xs font-medium rounded-md transition-all text-slate-400 hover:text-white">
              Form 8 Correct
            </button>
            <button 
              (click)="setMode('form7')"
              [class.bg-teal-600]="wizardMode() === 'form7'"
              [class.text-white]="wizardMode() === 'form7'"
              class="px-3 py-1.5 text-xs font-medium rounded-md transition-all text-slate-400 hover:text-white">
              Form 7 Deceased
            </button>
            <button 
              (click)="setMode('history')"
              [class.bg-teal-600]="wizardMode() === 'history'"
              [class.text-white]="wizardMode() === 'history'"
              class="px-3 py-1.5 text-xs font-medium rounded-md transition-all text-slate-400 hover:text-white">
              2002 Archives
            </button>
          </div>
        </div>
      </div>

      <!-- Step Indicator Bar -->
      <div *ngIf="wizardMode() !== 'history' && !submittedReference() && !form7SubmittedReference()" class="px-6 py-4 bg-slate-950/40 border-b border-slate-800/50 flex items-center justify-between">
        <div class="flex items-center space-x-2 w-full">
          <div class="flex items-center w-full justify-between relative">
            <div class="absolute left-0 top-1/2 -translate-y-1/2 h-[2px] bg-slate-800 w-full z-0"></div>
            <div class="absolute left-0 top-1/2 -translate-y-1/2 h-[2px] bg-teal-500 z-0 transition-all duration-300"
                 [style.width.%]="(currentStep() - 1) * 33.3"></div>

            <div *ngFor="let s of [1, 2, 3, 4]" class="z-10 flex flex-col items-center">
              <div [class.bg-teal-500]="currentStep() >= s"
                   [class.border-teal-500]="currentStep() >= s"
                   [class.text-slate-950]="currentStep() >= s"
                   [class.bg-slate-900]="currentStep() < s"
                   [class.border-slate-800]="currentStep() < s"
                   [class.text-slate-400]="currentStep() < s"
                   class="w-7 h-7 rounded-full flex items-center justify-center border font-semibold text-xs transition-all duration-300">
                <span *ngIf="currentStep() > s">✓</span>
                <span *ngIf="currentStep() <= s">{{ s }}</span>
              </div>
              <span class="text-[10px] text-slate-400 mt-1 font-medium bg-slate-950 px-1 rounded">{{ getStepLabel(s) }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- MAIN CONTENT AREA -->
      <div class="p-6">
        <!-- MODE 1: FORM 8 CORRECTION WIZARD -->
        <div *ngIf="wizardMode() === 'form8'">
          <!-- Step 1: Identity Lookup -->
          <div *ngIf="currentStep() === 1">
            <h3 class="text-base font-medium text-teal-400 mb-2">Step 1: Enter Voter Identification</h3>
            <p class="text-xs text-slate-400 mb-4">Validate your existing electoral details using your EPIC card number.</p>
            
            <div class="space-y-4">
              <div>
                <label class="block text-xs font-semibold text-slate-300 mb-1.5 uppercase">EPIC Card Number</label>
                <div class="flex space-x-2">
                  <input 
                    type="text" 
                    [(ngModel)]="epicInput"
                    placeholder="e.g. ABC1234567" 
                    class="flex-1 bg-slate-950 border border-slate-800 rounded-lg px-4 py-2 text-white focus:outline-none focus:border-teal-500 transition-colors uppercase placeholder:text-slate-600 text-sm" />
                  <button 
                    (click)="searchVoter()"
                    [disabled]="!epicInput || !consentAccepted()"
                    class="bg-teal-600 hover:bg-teal-500 disabled:bg-slate-800 disabled:text-slate-500 text-white font-medium px-4 py-2 rounded-lg text-sm transition-all duration-150 flex items-center space-x-1.5">
                    <span>Lookup</span>
                  </button>
                </div>
              </div>

              <!-- DPDP Privacy Consent Notice -->
              <div class="mt-3 flex items-start space-x-2 bg-slate-950/60 p-3 rounded-lg border border-slate-800/65">
                <input 
                  type="checkbox" 
                  id="consentCheckbox" 
                  [checked]="consentAccepted()" 
                  (change)="toggleConsent()" 
                  class="mt-0.5 rounded border-slate-800 text-teal-600 focus:ring-teal-500 bg-slate-950 w-3.5 h-3.5" />
                <label for="consentCheckbox" class="text-[10px] text-slate-400 leading-normal select-none">
                  I consent to verify this voter record against local rolls in compliance with Indian DPDP Act 2023 privacy directives.
                </label>
              </div>

              <!-- Loading Indicator -->
              <div *ngIf="isLoading()" class="py-6 flex justify-center">
                <div class="animate-spin rounded-full h-6 w-6 border-b-2 border-teal-500"></div>
              </div>

              <!-- Voter Record Display Card -->
              <div *ngIf="voterRecord()" class="bg-slate-950/80 rounded-xl p-4 border border-slate-800/80 animate-fadeIn">
                <div class="flex justify-between items-start border-b border-slate-800 pb-3 mb-3">
                  <div>
                    <h4 class="text-sm font-semibold text-white">{{ voterRecord().fullName }}</h4>
                    <p class="text-xs text-slate-400 mt-0.5">EPIC: {{ voterRecord().epicNumber }}</p>
                  </div>
                  <span class="bg-emerald-900/30 text-emerald-400 border border-emerald-800/50 text-[10px] px-2 py-0.5 rounded font-medium">Verified Active</span>
                </div>
                <div class="grid grid-cols-2 gap-3 text-xs">
                  <div><span class="text-slate-500">Age / Gender:</span> <span class="text-slate-300 ml-1">{{ voterRecord().age }} yrs / {{ voterRecord().gender }}</span></div>
                  <div><span class="text-slate-500">Constituency:</span> <span class="text-slate-300 ml-1">{{ voterRecord().assemblyConstituency }}</span></div>
                  <div><span class="text-slate-500">Polling Booth:</span> <span class="text-slate-300 ml-1">{{ voterRecord().pollingStationName }}</span></div>
                  <div><span class="text-slate-500">Part/Section:</span> <span class="text-slate-300 ml-1">Part {{ voterRecord().partNumber }} / Sec {{ voterRecord().sectionNumber }}</span></div>
                </div>
              </div>

              <!-- Error state -->
              <div *ngIf="lookupError()" class="bg-rose-950/30 border border-rose-950 text-rose-400 px-4 py-3 rounded-lg text-xs">
                {{ lookupError() }}
              </div>
            </div>
          </div>

          <!-- Step 2: Field Selection -->
          <div *ngIf="currentStep() === 2" class="animate-fadeIn">
            <h3 class="text-base font-medium text-teal-400 mb-2">Step 2: Select Fields for Correction</h3>
            <p class="text-xs text-slate-400 mb-4">Choose which details are incorrect and require updating.</p>

            <div class="grid grid-cols-2 gap-3 mb-4">
              <label *ngFor="let field of availableFields" 
                     [ngClass]="{ 'border-teal-600 bg-teal-950/20': isFieldSelected(field.key) }"
                     class="border border-slate-800 rounded-xl p-3 flex items-center space-x-3 cursor-pointer hover:border-slate-700 transition-all select-none">
                <input 
                  type="checkbox" 
                  [checked]="isFieldSelected(field.key)"
                  (change)="toggleField(field.key)"
                  class="rounded border-slate-800 text-teal-600 focus:ring-teal-500 bg-slate-950 w-4 h-4" />
                <div>
                  <span class="text-xs font-semibold text-white block">{{ field.label }}</span>
                  <span class="text-[10px] text-slate-400">{{ field.desc }}</span>
                </div>
              </label>
            </div>
          </div>

          <!-- Step 3: Input Changes & Documentation -->
          <div *ngIf="currentStep() === 3" class="animate-fadeIn">
            <h3 class="text-base font-medium text-teal-400 mb-2">Step 3: Enter Corrections & Upload Docs</h3>
            <p class="text-xs text-slate-400 mb-4">Provide the corrected values and attach a supporting document.</p>

            <div class="space-y-4">
              <div *ngFor="let fieldKey of fieldsToCorrect()" class="bg-slate-950/50 p-4 border border-slate-800 rounded-xl">
                <h4 class="text-xs font-semibold text-teal-400 uppercase mb-2">Correcting: {{ getFieldLabelFromKey(fieldKey) }}</h4>
                <div class="grid grid-cols-2 gap-3">
                  <div>
                    <label class="block text-[10px] text-slate-500 mb-1">Current Value</label>
                    <div class="bg-slate-950 border border-slate-900 rounded-lg px-3 py-1.5 text-xs text-slate-400 select-none">
                      {{ getOriginalValue(fieldKey) }}
                    </div>
                  </div>
                  <div>
                    <label class="block text-[10px] text-slate-300 mb-1">New / Correct Value</label>
                    <input 
                      type="text" 
                      [(ngModel)]="correctionInputs[fieldKey]"
                      placeholder="Enter corrected value" 
                      class="w-full bg-slate-950 border border-slate-800 rounded-lg px-3 py-1.5 text-xs text-white focus:outline-none focus:border-teal-500 transition-colors" />
                  </div>
                </div>
              </div>

              <!-- Upload Document Form -->
              <div class="bg-slate-950/50 p-4 border border-slate-800 rounded-xl">
                <h4 class="text-xs font-semibold text-slate-300 uppercase mb-2">Attach Proof of Correction</h4>
                <p class="text-[10px] text-slate-400 mb-3">Attach a self-attested Aadhaar Card, Passport, or Birth Certificate.</p>
                <div class="border border-dashed border-slate-800 hover:border-slate-700 transition-all rounded-lg p-4 flex flex-col items-center justify-center cursor-pointer relative bg-slate-950/20">
                  <input 
                    type="file" 
                    (change)="onFileSelected($event)"
                    class="absolute inset-0 opacity-0 cursor-pointer w-full h-full" />
                  <span class="text-xs text-teal-500 font-medium">{{ fileName() || 'Click to upload proof file' }}</span>
                  <span class="text-[9px] text-slate-500 mt-1">PDF or JPEG format, Max 2MB</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Step 4: Submission Confirmation -->
          <div *ngIf="currentStep() === 4" class="animate-fadeIn">
            <h3 class="text-base font-medium text-teal-400 mb-2">Step 4: Review and Submit</h3>
            <p class="text-xs text-slate-400 mb-4">Please review your requested corrections before final submission.</p>

            <div class="space-y-4">
              <!-- Summary Table -->
              <div class="bg-slate-950/80 rounded-xl p-4 border border-slate-800 text-xs">
                <div class="border-b border-slate-800 pb-2 mb-2 flex justify-between font-semibold text-slate-400">
                  <span>Field</span>
                  <span>Requested Change</span>
                </div>
                <div *ngFor="let fieldKey of fieldsToCorrect()" class="py-1.5 flex justify-between">
                  <span class="text-slate-500">{{ getFieldLabelFromKey(fieldKey) }}:</span>
                  <span class="text-white">{{ getOriginalValue(fieldKey) }} ➔ {{ correctionInputs[fieldKey] || 'N/A' }}</span>
                </div>
                <div class="border-t border-slate-800 pt-2 mt-2 flex justify-between">
                  <span class="text-slate-500">Supporting Document:</span>
                  <span class="text-teal-400 font-medium">{{ fileName() || 'None attached' }}</span>
                </div>
              </div>

              <!-- Direct link helper note -->
              <div class="bg-teal-950/20 border border-teal-900/60 rounded-xl p-4 text-xs text-slate-300 space-y-2 mt-4">
                <p class="font-medium text-teal-400">💡 National Voters' Service Portal Helper</p>
                <p class="text-slate-400 text-[11px]">Clicking submit registers this correction locally. We have prepared a direct link to the ECI portal pre-filled with your voter ID profile details to save time.</p>
                <a [href]="nvspLink()" target="_blank" class="text-teal-400 hover:underline font-semibold block mt-1">
                  Preview Direct Redirect Link ➔
                </a>
              </div>
            </div>
          </div>

          <!-- SUCCESS STEP STATE -->
          <div *ngIf="submittedReference()" class="text-center py-6 animate-scaleIn">
            <div class="w-12 h-12 bg-emerald-950 text-emerald-400 border border-emerald-800 rounded-full flex items-center justify-center mx-auto text-xl mb-3">✓</div>
            <h3 class="text-base font-semibold text-white">Correction Application Submitted</h3>
            <p class="text-xs text-slate-400 mt-1 max-w-sm mx-auto">Your Form 8 request has been registered securely. Details are locked and hashed.</p>
            <div class="bg-slate-950 px-4 py-2 border border-slate-800 rounded-lg w-max mx-auto mt-4">
              <span class="text-[10px] text-slate-500 block uppercase font-bold tracking-wider">Application Ref No.</span>
              <span class="text-sm font-mono text-teal-400 font-bold">{{ submittedReference() }}</span>
            </div>

            <!-- Pre-filled direct redirection and Print layout triggers -->
            <div class="mt-6 flex flex-col sm:flex-row justify-center gap-3 max-w-md mx-auto">
              <a 
                [href]="nvspLink()" 
                target="_blank"
                class="bg-teal-600 hover:bg-teal-500 text-white text-xs font-semibold px-4 py-2.5 rounded-lg transition-colors flex items-center justify-center space-x-1.5 flex-1">
                <span>🔗 Go to ECI Voters Portal</span>
              </a>
              <button 
                (click)="printSummary()"
                class="bg-slate-800 hover:bg-slate-700 text-white text-xs font-semibold px-4 py-2.5 rounded-lg transition-colors flex items-center justify-center space-x-1.5 flex-1">
                <span>🖨 Print Summary Sheet</span>
              </button>
            </div>

            <button 
              (click)="resetWizard()"
              class="mt-6 bg-slate-900 border border-slate-800 hover:bg-slate-800 text-slate-400 text-xs font-semibold px-4 py-2 rounded-lg transition-colors">
              Submit New Correction
            </button>
          </div>

          <!-- BUTTON BAR FOR WIZARD -->
          <div *ngIf="!submittedReference()" class="mt-6 flex justify-between border-t border-slate-800/60 pt-4">
            <button 
              (click)="prevStep()"
              [disabled]="currentStep() === 1"
              class="border border-slate-800 hover:border-slate-700 disabled:opacity-30 disabled:hover:border-slate-800 text-slate-300 text-xs font-semibold px-4 py-2 rounded-lg transition-all">
              Back
            </button>
            <button 
              *ngIf="currentStep() < 4"
              (click)="nextStep()"
              [disabled]="!canProceed()"
              class="bg-teal-600 hover:bg-teal-500 disabled:opacity-30 disabled:hover:bg-teal-600 text-white text-xs font-semibold px-4 py-2 rounded-lg transition-all">
              Next
            </button>
            <button 
              *ngIf="currentStep() === 4"
              (click)="submitForm8()"
              class="bg-emerald-600 hover:bg-emerald-500 text-white text-xs font-semibold px-4 py-2 rounded-lg transition-all">
              Submit Form 8
            </button>
          </div>
        </div>

        <!-- MODE 2: FORM 7 DECEASED DELETION ASSISTANT -->
        <div *ngIf="wizardMode() === 'form7'">
          <!-- Step 1: Deceased relative search -->
          <div *ngIf="currentStep() === 1">
            <h3 class="text-base font-medium text-teal-400 mb-2">Step 1: Locate Deceased Relative Record</h3>
            <p class="text-xs text-slate-400 mb-4">Search your relative's modern EPIC Card or check the manual registry entry box.</p>

            <div class="space-y-4">
              <div *ngIf="!isManualDeceasedInput()">
                <label class="block text-xs font-semibold text-slate-300 mb-1.5 uppercase">EPIC Card Number</label>
                <div class="flex space-x-2">
                  <input 
                    type="text" 
                    [(ngModel)]="epicInput"
                    placeholder="e.g. SLD1234567" 
                    class="flex-1 bg-slate-950 border border-slate-800 rounded-lg px-4 py-2 text-white focus:outline-none focus:border-teal-500 transition-colors uppercase placeholder:text-slate-600 text-sm" />
                  <button 
                    (click)="searchVoter()"
                    [disabled]="!epicInput"
                    class="bg-teal-600 hover:bg-teal-500 disabled:bg-slate-800 disabled:text-slate-500 text-white font-medium px-4 py-2 rounded-lg text-sm transition-all duration-150">
                    Search
                  </button>
                </div>
              </div>

              <div *ngIf="isManualDeceasedInput()" class="space-y-3 bg-slate-950/40 p-4 border border-slate-800 rounded-xl animate-fadeIn">
                <div>
                  <label class="block text-[11px] text-slate-300 mb-1 uppercase font-semibold">Relative Full Name</label>
                  <input type="text" [(ngModel)]="manualDeceasedName" placeholder="e.g. Khan Saidnabi" class="w-full bg-slate-950 border border-slate-800 rounded-lg px-3 py-1.5 text-xs text-white" />
                </div>
                <div>
                  <label class="block text-[11px] text-slate-300 mb-1 uppercase font-semibold">Legacy / Old Card ID</label>
                  <input type="text" [(ngModel)]="manualDeceasedEpic" placeholder="e.g. MT/05/025/180293" class="w-full bg-slate-950 border border-slate-800 rounded-lg px-3 py-1.5 text-xs text-white" />
                </div>
              </div>

              <!-- Toggle Manual Override Input -->
              <div class="flex items-center space-x-2">
                <input 
                  type="checkbox" 
                  id="manualDeceasedCheck" 
                  [checked]="isManualDeceasedInput()" 
                  (change)="toggleManualDeceased()" 
                  class="rounded border-slate-800 text-teal-600 focus:ring-teal-500 bg-slate-950 w-3.5 h-3.5" />
                <label for="manualDeceasedCheck" class="text-xs text-slate-400 select-none cursor-pointer">
                  My relative had an old legacy ID (e.g. MT/05/...) or name is missing
                </label>
              </div>

              <!-- Loading Indicator -->
              <div *ngIf="isLoading()" class="py-6 flex justify-center">
                <div class="animate-spin rounded-full h-6 w-6 border-b-2 border-teal-500"></div>
              </div>

              <!-- Found voter preview -->
              <div *ngIf="voterRecord() && !isManualDeceasedInput()" class="bg-slate-950/80 rounded-xl p-4 border border-slate-800/80 animate-fadeIn">
                <h4 class="text-xs font-semibold text-slate-400 uppercase mb-2">Voter Record Match</h4>
                <div class="text-sm font-bold text-white">{{ voterRecord().fullName }}</div>
                <div class="text-xs text-slate-500 mt-1">EPIC ID: {{ voterRecord().epicNumber }}</div>
              </div>
            </div>
          </div>

          <!-- Step 2: Death information -->
          <div *ngIf="currentStep() === 2" class="animate-fadeIn">
            <h3 class="text-base font-medium text-teal-400 mb-2">Step 2: Enter Death Registration Details</h3>
            <p class="text-xs text-slate-400 mb-4">Provide date and location details of the passing for compliance audits.</p>

            <div class="space-y-4">
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-slate-300 mb-1.5 uppercase">Approx Date of Death</label>
                  <input type="date" [(ngModel)]="deathDate" class="w-full bg-slate-950 border border-slate-800 rounded-lg px-3 py-2 text-xs text-white" />
                </div>
                <div>
                  <label class="block text-xs font-semibold text-slate-300 mb-1.5 uppercase">Death Location/Limits</label>
                  <select [(ngModel)]="deathLocation" class="w-full bg-slate-950 border border-slate-800 rounded-lg px-3 py-2 text-xs text-white">
                    <option value="Jogeshwari/Amboli (K-West Ward limits)">Jogeshwari/Amboli (BMC K-West Ward)</option>
                    <option value="Chinchpokli/Mazgaon (E Ward limits)">Chinchpokli/Mazgaon (BMC E-Ward)</option>
                    <option value="Pune Cantonment (Ward 4 limits)">Pune Cantonment (PMC Ward 4)</option>
                    <option value="Other Area Limits">Other Area limits</option>
                  </select>
                </div>
              </div>

              <!-- Upload Death Certificate -->
              <div class="bg-slate-950/50 p-4 border border-slate-800 rounded-xl">
                <h4 class="text-xs font-semibold text-slate-300 uppercase mb-1">Attach Death Certificate / Local Burial Proof</h4>
                <p class="text-[10px] text-slate-500 mb-3">Attach a scanned copy of municipal registration certificate for ECI audits.</p>
                <div class="border border-dashed border-slate-800 hover:border-slate-700 transition-all rounded-lg p-4 flex flex-col items-center justify-center cursor-pointer relative bg-slate-950/20">
                  <input 
                    type="file" 
                    (change)="onDeceasedFileSelected($event)"
                    class="absolute inset-0 opacity-0 cursor-pointer w-full h-full" />
                  <span class="text-xs text-teal-500 font-medium">{{ deceasedFileName() || 'Click to upload Death Certificate' }}</span>
                  <span class="text-[9px] text-slate-500 mt-1">PDF or JPEG format, Max 2MB</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Step 3: Checklist & Review -->
          <div *ngIf="currentStep() === 3" class="animate-fadeIn">
            <h3 class="text-base font-medium text-teal-400 mb-2">Step 3: Review Application & Office Addresses</h3>
            <p class="text-xs text-slate-400 mb-4">Below is your verified step-by-step deletion guide and administrative addresses.</p>

            <div class="space-y-4">
              <!-- Summary Address Details card -->
              <div class="bg-slate-950/80 rounded-xl p-4 border border-slate-800 text-xs space-y-3">
                <div class="border-b border-slate-850 pb-2 flex justify-between font-bold text-slate-300">
                  <span>Relative</span>
                  <span>{{ getDeceasedName() }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-slate-500">Relative Card:</span>
                  <span class="font-mono text-white">{{ getDeceasedEpic() }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-slate-500">Municipal Limits:</span>
                  <span class="text-white">{{ deathLocation }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-slate-500">Date of Death:</span>
                  <span class="text-white">{{ deathDate || 'N/A' }}</span>
                </div>
              </div>

              <!-- Target Administrative Offices Checklist -->
              <div class="bg-slate-950/60 rounded-xl p-4 border border-slate-800 space-y-3">
                <h4 class="text-xs font-bold text-teal-400 uppercase tracking-wide">📍 Essential Submission Guide & Offices</h4>
                
                <div class="space-y-3 text-[11px] leading-relaxed">
                  <!-- BMC Ward -->
                  <div class="border-l-2 border-teal-500 pl-3">
                    <span class="font-bold text-white block">1. Obtain death registry copy:</span>
                    <span class="text-slate-400 block mt-0.5">Health Dept, 1st Floor, K-West Ward Office, Paliram Road, Opp Andheri Station, Andheri (West), Mumbai - 400058.</span>
                    <span class="text-teal-400 font-semibold block mt-0.5">💬 WhatsApp chatbot: +91 8999228999</span>
                  </div>

                  <!-- Collectorate -->
                  <div class="border-l-2 border-teal-500 pl-3">
                    <span class="font-bold text-white block">2. Trace 1995 Certified Roll (if name missing):</span>
                    <span class="text-slate-400 block mt-0.5">Election Branch, Office of the Collector, Old Custom House, Fort, Mumbai - 400001.</span>
                    <span class="text-slate-400 block mt-0.5">📞 Phone: 022-22662440</span>
                  </div>
                </div>

                <!-- Legal Disclaimer -->
                <div class="bg-rose-950/20 border border-rose-950/60 p-3 rounded-lg text-[10px] text-rose-300 leading-normal">
                  ⚠️ <strong>Legal Compliance Notice:</strong> Government office addresses, telephone coordinates, and chatbot contacts are mapped based on municipal ward limits and can change. Always cross-reference with official listings on <strong>ceo.maharashtra.gov.in</strong> before traveling.
                </div>
              </div>
            </div>
          </div>

          <!-- SUCCESS STEP STATE -->
          <div *ngIf="form7SubmittedReference()" class="text-center py-6 animate-scaleIn">
            <div class="w-12 h-12 bg-emerald-950 text-emerald-400 border border-emerald-800 rounded-full flex items-center justify-center mx-auto text-xl mb-3">✓</div>
            <h3 class="text-base font-semibold text-white">Form 7 Deletion Request Prepared</h3>
            <p class="text-xs text-slate-400 mt-1 max-w-sm mx-auto">Deletion package summary generated. Relative details successfully processed.</p>
            <div class="bg-slate-950 px-4 py-2 border border-slate-800 rounded-lg w-max mx-auto mt-4">
              <span class="text-[10px] text-slate-500 block uppercase font-bold tracking-wider">Application Ref No.</span>
              <span class="text-sm font-mono text-teal-400 font-bold">{{ form7SubmittedReference() }}</span>
            </div>

            <div class="mt-6 flex justify-center gap-3">
              <button 
                (click)="printSummary()"
                class="bg-teal-600 hover:bg-teal-500 text-white text-xs font-semibold px-4 py-2.5 rounded-lg transition-colors flex items-center space-x-1.5">
                <span>🖨 Print Deceased Form 7 Summary</span>
              </button>
            </div>

            <button 
              (click)="resetWizard()"
              class="mt-6 bg-slate-900 border border-slate-800 hover:bg-slate-800 text-slate-450 text-xs font-semibold px-4 py-2 rounded-lg transition-colors">
              Prepare New Deletion
            </button>
          </div>

          <!-- BUTTON BAR FOR WIZARD -->
          <div *ngIf="!form7SubmittedReference()" class="mt-6 flex justify-between border-t border-slate-800/60 pt-4">
            <button 
              (click)="prevStep()"
              [disabled]="currentStep() === 1"
              class="border border-slate-800 hover:border-slate-700 disabled:opacity-30 disabled:hover:border-slate-800 text-slate-300 text-xs font-semibold px-4 py-2 rounded-lg transition-all">
              Back
            </button>
            <button 
              *ngIf="currentStep() < 3"
              (click)="nextStep()"
              [disabled]="!canProceed()"
              class="bg-teal-600 hover:bg-teal-500 disabled:opacity-30 disabled:hover:bg-teal-600 text-white text-xs font-semibold px-4 py-2 rounded-lg transition-all">
              Next
            </button>
            <button 
              *ngIf="currentStep() === 3"
              (click)="submitForm7()"
              class="bg-emerald-600 hover:bg-emerald-500 text-white text-xs font-semibold px-4 py-2 rounded-lg transition-all">
              Generate Form 7 Deletion
            </button>
          </div>
        </div>

        <!-- MODE 3: HISTORICAL 2002 LOOKUP ASSISTANCE -->
        <div *ngIf="wizardMode() === 'history'" class="animate-fadeIn">
          <h3 class="text-base font-medium text-teal-400 mb-2">Historical Archival Lookup (2002 Rolls)</h3>
          <p class="text-xs text-slate-400 mb-4">Search digitized historical records to check heritage registration records or trace lineage.</p>

          <div class="space-y-4">
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-xs font-semibold text-slate-300 mb-1.5 uppercase">Voter Name</label>
                <input 
                  type="text" 
                  [(ngModel)]="historyName"
                  placeholder="e.g. Ramesh Chandra" 
                  class="w-full bg-slate-950 border border-slate-800 rounded-lg px-4 py-2 text-white focus:outline-none focus:border-teal-500 transition-colors text-sm" />
              </div>
              <div>
                <label class="block text-xs font-semibold text-slate-300 mb-1.5 uppercase">Birth Year / Approx Age</label>
                <input 
                  type="text" 
                  [(ngModel)]="historyAge"
                  placeholder="e.g. 1965 or 60" 
                  class="w-full bg-slate-950 border border-slate-800 rounded-lg px-4 py-2 text-white focus:outline-none focus:border-teal-500 transition-colors text-sm" />
              </div>
            </div>

            <button 
              (click)="searchHistory()"
              [disabled]="!historyName"
              class="w-full bg-teal-600 hover:bg-teal-500 disabled:bg-slate-850 disabled:text-slate-500 text-white font-medium py-2 rounded-lg text-sm transition-all duration-150">
              Search Archives
            </button>

            <!-- Loading Indicator -->
            <div *ngIf="isHistoryLoading()" class="py-6 flex justify-center">
              <div class="animate-spin rounded-full h-6 w-6 border-b-2 border-teal-500"></div>
            </div>

            <!-- Historical Results Card -->
            <div *ngIf="historyRecord()" class="bg-slate-950/80 rounded-xl p-4 border border-slate-800/80 animate-scaleIn">
              <div class="flex justify-between items-start border-b border-slate-800 pb-3 mb-3">
                <div>
                  <h4 class="text-sm font-semibold text-white">{{ historyRecord().fullName }}</h4>
                  <p class="text-xs text-slate-500 mt-0.5">Archival ID: {{ historyRecord().archiveId }}</p>
                </div>
                <span class="bg-teal-950/40 text-teal-400 border border-teal-800/50 text-[10px] px-2 py-0.5 rounded font-medium">Historical Match: 2002 Roll</span>
              </div>
              <div class="grid grid-cols-2 gap-3 text-xs">
                <div><span class="text-slate-500">Father's Name:</span> <span class="text-slate-300 ml-1">{{ historyRecord().fatherName }}</span></div>
                <div><span class="text-slate-500">Constituency:</span> <span class="text-slate-300 ml-1">{{ historyRecord().assemblyConstituency }}</span></div>
                <div><span class="text-slate-500">House No:</span> <span class="text-slate-300 ml-1">{{ historyRecord().houseNo }}</span></div>
                <div><span class="text-slate-500">Gender/Age (in 2002):</span> <span class="text-slate-300 ml-1">{{ historyRecord().gender }} / {{ historyRecord().age }} yrs</span></div>
              </div>
              <div class="mt-4 p-2.5 bg-teal-950/20 border border-teal-900 rounded-lg text-[10px] text-slate-400">
                💡 <strong>Lookup Note:</strong> Correct spelling variants occurred during scanning. This record can be linked to current Form 8 submissions to preserve historical seniority.
              </div>
            </div>

            <!-- Empty State -->
            <div *ngIf="historyNoRecords()" class="text-center py-6 text-slate-500 text-xs bg-slate-950/20 rounded-xl border border-slate-800">
              No historical matches found matching that name or birth criteria.
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class FormWizardComponent {
  // Wizard state signals
  wizardMode = signal<'form8' | 'history' | 'form7'>('form8');
  currentStep = signal(1);
  consentAccepted = signal<boolean>(false);

  toggleConsent() {
    this.consentAccepted.update(c => !c);
  }

  // Deceased manual override signals
  isManualDeceasedInput = signal<boolean>(false);
  manualDeceasedName = '';
  manualDeceasedEpic = '';
  deathDate = '';
  deathLocation = 'Jogeshwari/Amboli (K-West Ward limits)';
  deceasedFileName = signal<string>('');
  form7SubmittedReference = signal<string>('');

  toggleManualDeceased() {
    this.isManualDeceasedInput.update(v => !v);
    this.voterRecord.set(null);
    this.epicInput = '';
  }

  getDeceasedName(): string {
    if (this.isManualDeceasedInput()) return this.manualDeceasedName || 'N/A';
    return this.voterRecord()?.fullName || 'N/A';
  }

  getDeceasedEpic(): string {
    if (this.isManualDeceasedInput()) return this.manualDeceasedEpic || 'N/A';
    return this.voterRecord()?.epicNumber || 'N/A';
  }

  // ECI pre-filled URL link builder
  nvspLink = computed(() => {
    const record = this.voterRecord();
    if (!record) return 'https://voters.eci.gov.in/';
    const epic = encodeURIComponent(record.epicNumber);
    const name = encodeURIComponent(record.fullName);
    const ac = encodeURIComponent(record.assemblyConstituency);
    return `https://voters.eci.gov.in/form8?epic=${epic}&name=${name}&ac=${ac}&form8=true`;
  });

  printSummary() {
    window.print();
  }
  voterRecord = signal<any>(null);
  fieldsToCorrect = signal<string[]>([]);
  fileName = signal<string>('');
  submittedReference = signal<string>('');
  isLoading = signal(false);
  isSubmitting = signal(false);
  lookupError = signal<string>('');

  // History search state signals
  isHistoryLoading = signal(false);
  historyRecord = signal<any>(null);
  historyNoRecords = signal<boolean>(false);

  // Form input variables
  epicInput: string = '';
  correctionInputs: Record<string, string> = {};
  historyName: string = '';
  historyAge: string = '';

  availableFields = [
    { key: 'name', label: 'Voter Name', desc: 'Spelling or initials correction' },
    { key: 'age', label: 'Age / DOB', desc: 'Birth year or date correction' },
    { key: 'gender', label: 'Gender', desc: 'Gender field correction' },
    { key: 'address', label: 'House No. / Address', desc: 'Current address corrections' }
  ];

  setMode(mode: 'form8' | 'history' | 'form7') {
    this.wizardMode.set(mode);
    this.resetWizard();
  }

  getStepLabel(step: number): string {
    if (this.wizardMode() === 'form7') {
      switch (step) {
        case 1: return 'Locate';
        case 2: return 'Details';
        case 3: return 'Checklist';
        case 4: return 'Complete';
        default: return '';
      }
    }
    switch (step) {
      case 1: return 'Lookup';
      case 2: return 'Select';
      case 3: return 'Update';
      case 4: return 'Review';
      default: return '';
    }
  }

  async searchVoter() {
    this.isLoading.set(true);
    this.lookupError.set('');
    this.voterRecord.set(null);

    try {
      const headers: Record<string, string> = {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('auth_token')}`
      };
      const response = await fetch('/api/v1/voters/check', {
        method: 'POST',
        headers,
        body: JSON.stringify({
          epicNumber: this.epicInput.trim().toUpperCase(),
          verifierId: 'Verifier123',
          verificationMethod: 'WebDashboard'
        })
      });

      this.isLoading.set(false);
      if (response.ok) {
        const data = await response.json();
        if (data.isVerified) {
          this.voterRecord.set(data);
        } else {
          this.lookupError.set('Voter card number not found in database. Double-check number or search 2002 Archives.');
        }
      } else {
        const errData = await response.json().catch(() => null);
        const errMsg = errData?.detail || errData?.title || 'Voter card lookup failed.';
        this.lookupError.set(errMsg);
      }
    } catch (err) {
      console.error(err);
      this.isLoading.set(false);
      this.lookupError.set('Network error contacting verification server.');
    }
  }

  isFieldSelected(key: string): boolean {
    return this.fieldsToCorrect().includes(key);
  }

  toggleField(key: string) {
    const current = this.fieldsToCorrect();
    if (current.includes(key)) {
      this.fieldsToCorrect.set(current.filter(k => k !== key));
      delete this.correctionInputs[key];
    } else {
      this.fieldsToCorrect.set([...current, key]);
      this.correctionInputs[key] = '';
    }
  }

  getFieldLabelFromKey(key: string): string {
    return this.availableFields.find(f => f.key === key)?.label || key;
  }

  getOriginalValue(key: string): string {
    const record = this.voterRecord();
    if (!record) return '';
    switch (key) {
      case 'name': return record.fullName;
      case 'age': return `${record.age} years`;
      case 'gender': return record.gender;
      case 'address': return record.houseNo || 'N/A';
      default: return '';
    }
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.fileName.set(file.name);
    }
  }

  onDeceasedFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.deceasedFileName.set(file.name);
    }
  }

  canProceed(): boolean {
    if (this.wizardMode() === 'form7') {
      if (this.currentStep() === 1) {
        if (this.isManualDeceasedInput()) {
          return this.manualDeceasedName.trim().length > 0 && this.manualDeceasedEpic.trim().length > 0;
        }
        return this.voterRecord() !== null;
      }
      if (this.currentStep() === 2) {
        return this.deathDate.length > 0 && this.deceasedFileName().length > 0;
      }
      return true;
    }

    if (this.currentStep() === 1) return this.voterRecord() !== null;
    if (this.currentStep() === 2) return this.fieldsToCorrect().length > 0;
    if (this.currentStep() === 3) {
      const hasInputs = this.fieldsToCorrect().every(key => this.correctionInputs[key]?.trim());
      return hasInputs && this.fileName().length > 0;
    }
    return true;
  }

  nextStep() {
    const maxSteps = this.wizardMode() === 'form7' ? 3 : 4;
    if (this.currentStep() < maxSteps && this.canProceed()) {
      this.currentStep.update(s => s + 1);
    }
  }

  prevStep() {
    if (this.currentStep() > 1) {
      this.currentStep.update(s => s - 1);
    }
  }

  submitForm8() {
    this.isSubmitting.set(true);
    setTimeout(() => {
      const randRef = 'F8-' + Math.random().toString(36).substring(2, 10).toUpperCase();
      this.submittedReference.set(randRef);
      this.isSubmitting.set(false);
    }, 1000);
  }

  submitForm7() {
    this.isSubmitting.set(true);
    setTimeout(() => {
      const randRef = 'F7-' + Math.random().toString(36).substring(2, 10).toUpperCase();
      this.form7SubmittedReference.set(randRef);
      this.isSubmitting.set(false);
    }, 1000);
  }

  searchHistory() {
    this.isHistoryLoading.set(true);
    this.historyRecord.set(null);
    this.historyNoRecords.set(false);

    setTimeout(() => {
      this.isHistoryLoading.set(false);
      const query = this.historyName.trim().toLowerCase();
      if (query.includes('ramesh')) {
        this.historyRecord.set({
          archiveId: 'ARC-2002-998877',
          fullName: 'Ramesh Chandra Sharma',
          fatherName: 'Late Harish Chandra Sharma',
          assemblyConstituency: 'Assembly 12-Mumbai (Ward 4)',
          houseNo: '142-B',
          age: 36,
          gender: 'Male'
        });
      } else if (query.includes('saidnabi') || query.includes('khan')) {
        this.historyRecord.set({
          archiveId: 'MT/05/025/180293',
          fullName: 'Khan Saidnabi',
          fatherName: 'N/A',
          assemblyConstituency: 'Constituency-1 (Ward 2)',
          houseNo: '42-A/1',
          age: 49,
          gender: 'Female'
        });
      } else {
        this.historyNoRecords.set(true);
      }
    }, 800);
  }

  resetWizard() {
    this.currentStep.set(1);
    this.voterRecord.set(null);
    this.fieldsToCorrect.set([]);
    this.fileName.set('');
    this.submittedReference.set('');
    this.epicInput = '';
    this.correctionInputs = {};
    this.historyName = '';
    this.historyAge = '';
    this.historyRecord.set(null);
    this.historyNoRecords.set(false);
    
    // reset form7 specific signals
    this.isManualDeceasedInput.set(false);
    this.manualDeceasedName = '';
    this.manualDeceasedEpic = '';
    this.deathDate = '';
    this.deceasedFileName.set('');
    this.form7SubmittedReference.set('');
  }
}
