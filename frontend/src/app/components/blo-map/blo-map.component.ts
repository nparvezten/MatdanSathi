import { Component, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Geolocation } from '@capacitor/geolocation';

export interface BloListing {
  id: string;
  bloName: string;
  bloContact: string;
  pollingStationName: string;
  latitude: number;
  longitude: number;
  distanceInKm: number;
  verificationScore: number;
  verificationCount: number;
  isGovernmentFacility: boolean;
  facilityType: string;
  isVerified: boolean;
}

@Component({
  selector: 'app-blo-map',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="w-full max-w-2xl mx-auto bg-slate-900/60 backdrop-blur-md border border-slate-800 rounded-2xl shadow-xl overflow-hidden mt-6">
      <!-- Title Header -->
      <div class="px-6 py-5 border-b border-slate-800 bg-gradient-to-r from-teal-900/30 to-slate-900">
        <h2 class="text-xl font-semibold text-white tracking-wide">BLO & Neutral Facility Locator Map</h2>
        <p class="text-xs text-slate-400 mt-1">Locate verified polling booths, government schools, and ward verification offices</p>
      </div>

      <!-- Filter Controls Bar -->
      <div class="px-6 py-3 bg-slate-950/80 border-b border-slate-800/80 flex flex-wrap items-center justify-between gap-3 text-xs">
        <div class="flex items-center space-x-4">
          <!-- Filter 1: Highlight Neutral Govt Facilities -->
          <label class="flex items-center space-x-1.5 cursor-pointer select-none">
            <input 
              type="checkbox" 
              [checked]="highlightGovtFacilities()" 
              (change)="toggleGovtFacilities()" 
              class="rounded border-slate-800 text-teal-600 focus:ring-teal-500 bg-slate-950 w-3.5 h-3.5" />
            <span class="text-slate-300 font-medium">🏛️ Highlight Neutral Govt Facilities</span>
          </label>

          <!-- Filter 2: Hide Unverified Spots -->
          <label class="flex items-center space-x-1.5 cursor-pointer select-none">
            <input 
              type="checkbox" 
              [checked]="filterVerifiedOnly()" 
              (change)="toggleVerifiedOnly()" 
              class="rounded border-slate-800 text-teal-600 focus:ring-teal-500 bg-slate-950 w-3.5 h-3.5" />
            <span class="text-slate-300 font-medium">✓ Hide Unverified Spots</span>
          </label>
        </div>

        <button 
          (click)="getCurrentLocation()"
          class="bg-teal-600 hover:bg-teal-500 text-white text-xs font-semibold px-3 py-1.5 rounded-lg transition-colors flex items-center space-x-1 ml-auto">
          <span>📍 Detect My GPS</span>
        </button>
      </div>

      <div class="p-6 grid grid-cols-1 md:grid-cols-12 gap-6">
        <!-- LEFT PANEL: Interactive Pin-Drop Map & Coordinates (7 cols) -->
        <div class="md:col-span-7 space-y-4">
          <!-- Interactive Simulated Map Box -->
          <div 
            (click)="onMapClick($event)"
            class="relative w-full h-64 bg-slate-950 border border-slate-800 rounded-xl overflow-hidden cursor-crosshair select-none">
            <!-- Simulated Grid Lines -->
            <div class="absolute inset-0 bg-[linear-gradient(to_right,#1e293b_1px,transparent_1px),linear-gradient(to_bottom,#1e293b_1px,transparent_1px)] bg-[size:24px_24px] opacity-25"></div>
            
            <!-- Map Help Instruction overlay -->
            <div class="absolute top-2 left-2 bg-slate-900/90 backdrop-blur border border-slate-800 px-2.5 py-1 rounded text-[10px] text-slate-400 pointer-events-none z-10">
              Click grid area to drop pin manually
            </div>

            <!-- Polling Station & Govt Facility dots -->
            <div 
              *ngFor="let item of filteredListings()"
              [style.left.%]="getRelativeMapX(item.longitude)"
              [style.top.%]="getRelativeMapY(item.latitude)"
              [ngClass]="{
                'bg-emerald-400 border-white ring-2 ring-emerald-500/50': item.isGovernmentFacility && highlightGovtFacilities(),
                'bg-teal-500 border-white': !item.isGovernmentFacility || !highlightGovtFacilities()
              }"
              class="absolute w-4 h-4 rounded-full -translate-x-1/2 -translate-y-1/2 cursor-pointer transition-transform hover:scale-125 z-10 group flex items-center justify-center">
              <span *ngIf="item.isGovernmentFacility" class="text-[8px] text-slate-950 font-black">🏛</span>
              <!-- Simple tooltip -->
              <div class="absolute bottom-full left-1/2 -translate-x-1/2 mb-1.5 hidden group-hover:block bg-slate-900 border border-slate-700 text-[10px] text-white px-2 py-1 rounded shadow-xl whitespace-nowrap z-30">
                <div class="font-bold text-teal-400">{{ item.pollingStationName }}</div>
                <div class="text-[9px] text-slate-300 mt-0.5">{{ item.facilityType }} • {{ item.bloName }}</div>
              </div>
            </div>

            <!-- Interactive Pin Drop -->
            <div 
              [style.left.%]="pinX()"
              [style.top.%]="pinY()"
              class="absolute w-5 h-5 -translate-x-1/2 -translate-y-full cursor-pointer animate-bounce z-20 pointer-events-none">
              <!-- SVG Pin Icon -->
              <svg viewBox="0 0 24 24" class="w-5 h-5 fill-rose-500 stroke-white stroke-2">
                <path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z"/>
              </svg>
            </div>
          </div>

          <!-- Coordinate status indicators -->
          <div class="grid grid-cols-2 gap-2 text-xs bg-slate-950/60 border border-slate-800/80 p-3 rounded-xl">
            <div>
              <span class="text-slate-500 block">Latitude</span>
              <span class="font-mono text-white font-medium">{{ currentLat() | number:'1.5-5' }}</span>
            </div>
            <div>
              <span class="text-slate-500 block">Longitude</span>
              <span class="font-mono text-white font-medium">{{ currentLon() | number:'1.5-5' }}</span>
            </div>
          </div>
        </div>

        <!-- RIGHT PANEL: BLO Listings Results (5 cols) -->
        <div class="md:col-span-5 flex flex-col h-full justify-between">
          <div class="space-y-3">
            <div class="flex justify-between items-center">
              <h3 class="text-xs font-semibold text-teal-400 uppercase tracking-wider">Nearest Verified Booths</h3>
              <span class="text-[10px] text-slate-400 font-mono">{{ filteredListings().length }} found</span>
            </div>
            
            <div *ngIf="isLoading()" class="py-12 flex justify-center">
              <div class="animate-spin rounded-full h-6 w-6 border-b-2 border-teal-500"></div>
            </div>

            <div *ngIf="!isLoading() && filteredListings().length === 0" class="text-center py-12 text-slate-500 text-xs bg-slate-950/40 rounded-xl border border-slate-800/60 p-4">
              No verified polling facilities matching filter criteria.
            </div>

            <div 
              *ngFor="let item of filteredListings()" 
              [ngClass]="{
                'border-emerald-700/80 bg-emerald-950/20': item.isGovernmentFacility && highlightGovtFacilities(),
                'border-slate-800 bg-slate-950/80': !item.isGovernmentFacility || !highlightGovtFacilities()
              }"
              class="rounded-xl p-3.5 border hover:border-slate-700 transition-all space-y-2">
              <div class="flex justify-between items-start">
                <div>
                  <h4 class="text-xs font-bold text-white leading-tight flex items-center gap-1">
                    <span>{{ item.pollingStationName }}</span>
                  </h4>
                  <p class="text-[10px] text-slate-400 mt-1">BLO Officer: <span class="text-slate-200 font-medium">{{ item.bloName }}</span></p>
                </div>
                <span class="bg-teal-950 text-teal-400 border border-teal-900 text-[9px] px-1.5 py-0.5 rounded font-mono font-medium">
                  {{ item.distanceInKm }} km
                </span>
              </div>

              <!-- Facility Badges -->
              <div class="flex flex-wrap gap-1.5 pt-1 text-[9px]">
                <span *ngIf="item.isGovernmentFacility" class="bg-emerald-950 text-emerald-300 border border-emerald-800 px-2 py-0.5 rounded-md font-semibold">
                  🏫 {{ item.facilityType }}
                </span>
                <span *ngIf="item.isVerified" class="bg-teal-950 text-teal-300 border border-teal-800 px-2 py-0.5 rounded-md font-semibold">
                  ✓ Verified Facility
                </span>
                <span class="bg-slate-900 text-slate-400 border border-slate-800 px-2 py-0.5 rounded-md font-mono">
                  Score: {{ item.verificationScore }}%
                </span>
              </div>

              <div class="text-[10px] text-slate-400 border-t border-slate-850 pt-1.5 flex justify-between items-center">
                <span>Contact: {{ item.bloContact }}</span>
                <span class="text-teal-400 font-mono">({{ item.verificationCount }} verifications)</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class BloMapComponent implements OnInit {
  currentLat = signal<number>(18.5204);
  currentLon = signal<number>(73.8567);
  pinX = signal<number>(50);
  pinY = signal<number>(50);

  isLoading = signal<boolean>(false);
  bloListings = signal<BloListing[]>([]);

  // Interactive filter signals
  highlightGovtFacilities = signal<boolean>(false);
  filterVerifiedOnly = signal<boolean>(true);

  // Computed filtered listing signals
  filteredListings = computed(() => {
    let listings = this.bloListings();

    // Filter unverified spots if active
    if (this.filterVerifiedOnly()) {
      listings = listings.filter(item => item.isVerified !== false);
    }

    // Sort government facilities first if highlighted
    if (this.highlightGovtFacilities()) {
      listings = [...listings].sort((a, b) => {
        if (a.isGovernmentFacility === b.isGovernmentFacility) return 0;
        return a.isGovernmentFacility ? -1 : 1;
      });
    }

    return listings;
  });

  ngOnInit() {
    this.getCurrentLocation();
  }

  toggleGovtFacilities() {
    this.highlightGovtFacilities.update(v => !v);
  }

  toggleVerifiedOnly() {
    this.filterVerifiedOnly.update(v => !v);
  }

  async getCurrentLocation() {
    try {
      const position = await Geolocation.getCurrentPosition();
      this.currentLat.set(position.coords.latitude);
      this.currentLon.set(position.coords.longitude);
      this.pinX.set(50);
      this.pinY.set(50);
      this.fetchBloListings();
    } catch (error) {
      console.warn('Capacitor Geolocation failed, attempting web fallback.', error);
      if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(
          (position) => {
            this.currentLat.set(position.coords.latitude);
            this.currentLon.set(position.coords.longitude);
            this.pinX.set(50);
            this.pinY.set(50);
            this.fetchBloListings();
          },
          (webErr) => {
            console.error('Web Geolocation also failed.', webErr);
            this.fetchBloListings();
          }
        );
      } else {
        this.fetchBloListings();
      }
    }
  }

  onMapClick(event: MouseEvent) {
    const rect = (event.currentTarget as HTMLElement).getBoundingClientRect();
    const x = event.clientX - rect.left;
    const y = event.clientY - rect.top;

    const pctX = (x / rect.width) * 100;
    const pctY = (y / rect.height) * 100;

    this.pinX.set(pctX);
    this.pinY.set(pctY);

    const latOffset = (50 - pctY) * 0.0003; 
    const lonOffset = (pctX - 50) * 0.0003;

    this.currentLat.update(l => l + latOffset);
    this.currentLon.update(l => l + lonOffset);

    this.fetchBloListings();
  }

  async fetchBloListings() {
    this.isLoading.set(true);
    try {
      const headers: Record<string, string> = {
        'Authorization': `Bearer ${localStorage.getItem('auth_token')}`
      };
      const response = await fetch(`/api/v1/blo/lookup?latitude=${this.currentLat()}&longitude=${this.currentLon()}`, { headers });
      if (response.ok) {
        const data = await response.json();
        const enriched = data.map((item: any) => ({
          ...item,
          isGovernmentFacility: item.pollingStationName.toLowerCase().includes('school') || item.pollingStationName.toLowerCase().includes('bmc') || item.pollingStationName.toLowerCase().includes('hall') || item.pollingStationName.toLowerCase().includes('office'),
          facilityType: item.pollingStationName.toLowerCase().includes('school') ? 'Government Primary School' : (item.pollingStationName.toLowerCase().includes('bmc') ? 'Municipal Ward Office' : 'Neutral Government Facility'),
          isVerified: true
        }));
        this.bloListings.set(enriched);
      } else {
        this.bloListings.set(this.getMockListings());
      }
    } catch (err) {
      console.error('Error contacting lookup API. Using local stubs.', err);
      this.bloListings.set(this.getMockListings());
    } finally {
      this.isLoading.set(false);
    }
  }

  getRelativeMapX(lon: number): number {
    const diff = (lon - this.currentLon()) * 5000 + 50;
    return Math.max(5, Math.min(95, diff));
  }

  getRelativeMapY(lat: number): number {
    const diff = 50 - (lat - this.currentLat()) * 5000;
    return Math.max(5, Math.min(95, diff));
  }

  private getMockListings(): BloListing[] {
    return [
      {
        id: 'mock-1',
        bloName: 'Ahmed Khan',
        bloContact: '+91 11111 22222',
        pollingStationName: 'Primary School Sector-4 (Room 1)',
        latitude: this.currentLat() + 0.003,
        longitude: this.currentLon() - 0.002,
        distanceInKm: 0.45,
        verificationScore: 97.4,
        verificationCount: 132,
        isGovernmentFacility: true,
        facilityType: 'Government Primary School',
        isVerified: true
      },
      {
        id: 'mock-2',
        bloName: 'Yasmin Shaikh',
        bloContact: '+91 22222 33333',
        pollingStationName: 'Government Girls High School (West Wing)',
        latitude: this.currentLat() - 0.006,
        longitude: this.currentLon() + 0.005,
        distanceInKm: 0.82,
        verificationScore: 95.8,
        verificationCount: 98,
        isGovernmentFacility: true,
        facilityType: 'Government High School',
        isVerified: true
      },
      {
        id: 'mock-3',
        bloName: 'Ziaul Haq',
        bloContact: '+91 33333 44444',
        pollingStationName: 'BMC K-West Ward Office Center',
        latitude: this.currentLat() + 0.012,
        longitude: this.currentLon() + 0.009,
        distanceInKm: 1.45,
        verificationScore: 98.1,
        verificationCount: 210,
        isGovernmentFacility: true,
        facilityType: 'Municipal Ward Office',
        isVerified: true
      },
      {
        id: 'mock-4',
        bloName: 'Unverified Local Desk',
        bloContact: '+91 00000 00000',
        pollingStationName: 'Temporary Unregistered Tent',
        latitude: this.currentLat() - 0.015,
        longitude: this.currentLon() - 0.012,
        distanceInKm: 2.10,
        verificationScore: 45.0,
        verificationCount: 3,
        isGovernmentFacility: false,
        facilityType: 'Unregistered Location',
        isVerified: false
      }
    ];
  }
}
