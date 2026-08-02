import { Component, Input, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface ObjectionCaseStatusData {
  id: string;
  caseType: string;
  status: 'Draft' | 'Filed' | 'Acknowledged' | 'UnderReview' | 'Resolved' | 'Rejected';
  submittedAtUtc: string;
  lastStatusUpdateUtc: string;
  applicantName?: string;
  epicNumber?: string;
  eroNotes?: string;
}

export interface EscalationContactData {
  id: string;
  district: string;
  eroNameOffice: string;
  deoOfficeAddress: string;
  helplineNumber: string;
  officialPortalUrl: string;
}

@Component({
  selector: 'app-objection-tracker',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="bg-slate-900 border border-slate-800 rounded-xl p-4 text-slate-200 shadow-xl space-y-4 font-sans">
      <!-- Header -->
      <div class="flex items-center justify-between border-b border-slate-800 pb-3">
        <div class="flex items-center space-x-2">
          <span class="text-xl">📋</span>
          <div>
            <h3 class="text-sm font-bold text-slate-100">Objection & Correction Case Tracker</h3>
            <p class="text-[11px] text-slate-400">Track real-time status of Form 6/7/8 SIR objection filings</p>
          </div>
        </div>
        <span [class]="statusBadgeClass()" class="px-2.5 py-1 text-[11px] font-bold rounded-full border">
          {{ caseData()?.status || 'Filed' }}
        </span>
      </div>

      <!-- Case Information Summary -->
      <div class="grid grid-cols-2 gap-3 text-xs bg-slate-950/60 p-3 rounded-lg border border-slate-800/80">
        <div>
          <span class="text-slate-500 block text-[10px] uppercase font-semibold">Case Reference</span>
          <span class="font-mono text-slate-200 text-[11px] font-bold">{{ caseData()?.id || 'OBJ-2026-88192' }}</span>
        </div>
        <div>
          <span class="text-slate-500 block text-[10px] uppercase font-semibold">Filing Type</span>
          <span class="text-slate-200 font-semibold">{{ caseData()?.caseType || 'Correction (Form 8)' }}</span>
        </div>
        <div>
          <span class="text-slate-500 block text-[10px] uppercase font-semibold">Applicant</span>
          <span class="text-slate-300 font-medium">{{ caseData()?.applicantName || 'Applicant' }}</span>
        </div>
        <div>
          <span class="text-slate-500 block text-[10px] uppercase font-semibold">Last Updated</span>
          <span class="text-slate-400 font-mono text-[11px]">{{ (caseData()?.lastStatusUpdateUtc | date:'short') || 'Just now' }}</span>
        </div>
      </div>

      <!-- Lifecycle Stepper Workflow -->
      <div class="py-2">
        <div class="text-[11px] font-semibold text-slate-400 mb-2">Workflow Progression:</div>
        <div class="grid grid-cols-4 gap-1 text-center relative">
          <div *ngFor="let step of steps; let i = index" class="flex flex-col items-center">
            <div [class]="getStepCircleClass(step)" class="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold transition-all z-10">
              {{ i + 1 }}
            </div>
            <span [class]="getStepTextClass(step)" class="text-[10px] font-semibold mt-1">
              {{ step }}
            </span>
          </div>
        </div>
      </div>

      <!-- ERO Official Notes (if available) -->
      <div *ngIf="caseData()?.eroNotes" class="bg-amber-950/40 border border-amber-800/50 p-2.5 rounded-lg text-xs text-amber-200">
        <span class="font-bold block text-[11px] text-amber-400">📝 ERO Remarks:</span>
        <p class="mt-0.5 text-slate-300">{{ caseData()?.eroNotes }}</p>
      </div>

      <!-- Action Footer & Contact ERO Trigger -->
      <div class="flex items-center justify-between pt-2 border-t border-slate-800">
        <button 
          (click)="toggleEscalationModal()" 
          class="bg-indigo-600 hover:bg-indigo-500 text-white font-bold text-xs px-3.5 py-2 rounded-lg transition-all flex items-center space-x-1.5 shadow-md">
          <span>📞 Contact your DEO / ERO</span>
        </button>
        <span class="text-[10px] text-slate-500 font-mono">CEO Maharashtra Electoral Roll Portal</span>
      </div>

      <!-- Escalation Modal Drawer -->
      <div *ngIf="showEscalationModal()" class="fixed inset-0 bg-black/80 backdrop-blur-sm flex items-center justify-center p-4 z-50">
        <div class="bg-slate-900 border border-slate-700 rounded-xl p-5 max-w-md w-full shadow-2xl space-y-4">
          <div class="flex justify-between items-center border-b border-slate-800 pb-3">
            <h4 class="text-sm font-bold text-slate-100">🏛 District ERO/DEO Escalation Directory</h4>
            <button (click)="toggleEscalationModal()" class="text-slate-400 hover:text-white font-bold text-lg">&times;</button>
          </div>

          <!-- District Selection -->
          <div class="space-y-1">
            <label class="text-[11px] font-semibold text-slate-400">Select District:</label>
            <select 
              [ngModel]="selectedDistrict()" 
              (ngModelChange)="onDistrictChange($event)"
              class="w-full bg-slate-950 border border-slate-800 text-slate-200 text-xs rounded-lg p-2.5 focus:border-indigo-500 focus:outline-none">
              <option value="Mumbai City">Mumbai City</option>
              <option value="Mumbai Suburban">Mumbai Suburban</option>
              <option value="Thane">Thane</option>
              <option value="Pune">Pune</option>
              <option value="Nagpur">Nagpur</option>
            </select>
          </div>

          <!-- Escalation Contact Card -->
          <div *ngIf="escalationData()" class="bg-slate-950 border border-slate-800/80 rounded-lg p-3.5 space-y-2.5 text-xs">
            <div>
              <span class="text-slate-500 block text-[10px] uppercase font-bold">Electoral Registration Officer (ERO)</span>
              <span class="text-indigo-300 font-semibold">{{ escalationData()?.eroNameOffice }}</span>
            </div>
            <div>
              <span class="text-slate-500 block text-[10px] uppercase font-bold">DEO Office Address</span>
              <span class="text-slate-300">{{ escalationData()?.deoOfficeAddress }}</span>
            </div>
            <div>
              <span class="text-slate-500 block text-[10px] uppercase font-bold">Helpline Number</span>
              <span class="text-emerald-400 font-bold font-mono text-sm">{{ escalationData()?.helplineNumber }}</span>
            </div>
            <div class="pt-1">
              <a 
                [href]="escalationData()?.officialPortalUrl" 
                target="_blank" 
                rel="noopener"
                class="text-indigo-400 hover:underline text-[11px] font-semibold flex items-center space-x-1">
                <span>🌐 Visit District Election Web Portal ↗</span>
              </a>
            </div>
          </div>

          <div class="flex justify-end">
            <button 
              (click)="toggleEscalationModal()" 
              class="bg-slate-800 hover:bg-slate-700 text-slate-200 text-xs px-4 py-2 rounded-lg font-semibold">
              Close
            </button>
          </div>
        </div>
      </div>
    </div>
  `
})
export class ObjectionTrackerComponent implements OnInit {
  @Input() caseId?: string;
  @Input() epicNumber?: string;

  caseData = signal<ObjectionCaseStatusData | null>(null);
  escalationData = signal<EscalationContactData | null>(null);
  showEscalationModal = signal<boolean>(false);
  selectedDistrict = signal<string>('Mumbai City');

  steps = ['Filed', 'Acknowledged', 'Under Review', 'Resolved'];

  ngOnInit() {
    if (this.caseId) {
      this.fetchCaseStatus(this.caseId);
    } else {
      this.setMockCaseData();
    }
    this.fetchEscalationContact(this.selectedDistrict());
  }

  async fetchCaseStatus(id: string) {
    try {
      const headers: Record<string, string> = {
        'Authorization': `Bearer ${localStorage.getItem('auth_token')}`
      };
      const res = await fetch(`/api/v1/objections/${id}`, { headers });
      if (res.ok) {
        const data = await res.json();
        this.caseData.set(data);
      } else {
        this.setMockCaseData();
      }
    } catch {
      this.setMockCaseData();
    }
  }

  async fetchEscalationContact(district: string) {
    try {
      const res = await fetch(`/api/v1/escalation/${encodeURIComponent(district)}`);
      if (res.ok) {
        const data = await res.json();
        this.escalationData.set(data);
      } else {
        this.setFallbackEscalationData(district);
      }
    } catch {
      this.setFallbackEscalationData(district);
    }
  }

  onDistrictChange(newDistrict: string) {
    this.selectedDistrict.set(newDistrict);
    this.fetchEscalationContact(newDistrict);
  }

  toggleEscalationModal() {
    this.showEscalationModal.update((v: boolean) => !v);
  }

  private setMockCaseData() {
    this.caseData.set({
      id: this.caseId || 'OBJ-2026-88192',
      caseType: 'Correction (Form 8)',
      status: 'Filed',
      submittedAtUtc: new Date().toISOString(),
      lastStatusUpdateUtc: new Date().toISOString(),
      applicantName: 'Khan Saidnabi',
      epicNumber: this.epicNumber || 'SLD1234567',
      eroNotes: 'Form 8 received by AERO Office. Awaiting document verification hearing.'
    });
  }

  private setFallbackEscalationData(district: string) {
    this.escalationData.set({
      id: 'mock-1',
      district: district,
      eroNameOffice: `ERO Electoral Registration Office ${district}`,
      deoOfficeAddress: `District Collectorate Office, ${district}, Maharashtra`,
      helplineNumber: '1950 / 022-22661234',
      officialPortalUrl: 'https://ceoelection.maharashtra.gov.in/'
    });
  }

  statusBadgeClass = computed(() => {
    const status = this.caseData()?.status || 'Filed';
    switch (status) {
      case 'Resolved': return 'bg-emerald-950/60 border-emerald-800 text-emerald-400';
      case 'UnderReview': return 'bg-blue-950/60 border-blue-800 text-blue-400';
      case 'Acknowledged': return 'bg-indigo-950/60 border-indigo-800 text-indigo-400';
      case 'Rejected': return 'bg-rose-950/60 border-rose-800 text-rose-400';
      default: return 'bg-amber-950/60 border-amber-800 text-amber-400';
    }
  });

  getStepCircleClass(stepName: string): string {
    const current = this.caseData()?.status || 'Filed';
    const currentIdx = this.getStepIndex(current);
    const stepIdx = this.getStepIndex(stepName);

    if (stepIdx < currentIdx) return 'bg-emerald-600 text-white border-emerald-500';
    if (stepIdx === currentIdx) return 'bg-indigo-600 text-white ring-2 ring-indigo-400 border-indigo-500 animate-pulse';
    return 'bg-slate-800 text-slate-500 border-slate-700';
  }

  getStepTextClass(stepName: string): string {
    const current = this.caseData()?.status || 'Filed';
    const currentIdx = this.getStepIndex(current);
    const stepIdx = this.getStepIndex(stepName);

    if (stepIdx <= currentIdx) return 'text-slate-200';
    return 'text-slate-500';
  }

  private getStepIndex(status: string): number {
    switch (status) {
      case 'Draft': return 0;
      case 'Filed': return 0;
      case 'Acknowledged': return 1;
      case 'UnderReview': return 2;
      case 'Resolved': return 3;
      case 'Rejected': return 3;
      default: return 0;
    }
  }
}
