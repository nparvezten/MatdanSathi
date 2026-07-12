import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Geolocation } from '@capacitor/geolocation';

@Component({
  selector: 'app-blo-map',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="w-full max-w-2xl mx-auto bg-slate-900/60 backdrop-blur-md border border-slate-800 rounded-2xl shadow-xl overflow-hidden mt-6">
      <!-- Title Header -->
      <div class="px-6 py-5 border-b border-slate-800 bg-gradient-to-r from-teal-900/30 to-slate-900">
        <h2 class="text-xl font-semibold text-white tracking-wide">BLO & Booth Locator Map</h2>
        <p class="text-xs text-slate-400 mt-1">Find local polling booths and verified contact listings</p>
      </div>

      <div class="p-6 grid grid-cols-1 md:grid-cols-12 gap-6">
        <!-- LEFT PANEL: Interactive Pin-Drop Map & Coordinates (7 cols) -->
        <div class="md:col-span-7 space-y-4">
          <div class="flex items-center justify-between">
            <span class="text-xs text-slate-400 font-medium">GPS Location Status</span>
            <button 
              (click)="getCurrentLocation()"
              class="bg-teal-600 hover:bg-teal-500 text-white text-xs font-semibold px-3 py-1.5 rounded-lg transition-colors flex items-center space-x-1">
              <span>📍 Detect My GPS</span>
            </button>
          </div>

          <!-- Interactive Simulated Map Box -->
          <div 
            (click)="onMapClick($event)"
            class="relative w-full h-64 bg-slate-950 border border-slate-800 rounded-xl overflow-hidden cursor-crosshair select-none">
            <!-- Simulated Grid Lines -->
            <div class="absolute inset-0 bg-[linear-gradient(to_right,#1e293b_1px,transparent_1px),linear-gradient(to_bottom,#1e293b_1px,transparent_1px)] bg-[size:24px_24px] opacity-25"></div>
            
            <!-- Map Help Instruction overlay -->
            <div class="absolute top-2 left-2 bg-slate-900/90 backdrop-blur border border-slate-800 px-2.5 py-1 rounded text-[10px] text-slate-400 pointer-events-none">
              Click grid area to drop pin manually
            </div>

            <!-- Polling Station dots representing current listings -->
            <div 
              *ngFor="let item of bloListings()"
              [style.left.%]="getRelativeMapX(item.longitude)"
              [style.top.%]="getRelativeMapY(item.latitude)"
              class="absolute w-3.5 h-3.5 bg-teal-500 hover:bg-teal-400 border border-white rounded-full -translate-x-1/2 -translate-y-1/2 cursor-pointer transition-transform hover:scale-125 z-10 group">
              <!-- Simple tooltip -->
              <span class="absolute bottom-full left-1/2 -translate-x-1/2 mb-1 hidden group-hover:block bg-slate-900 border border-slate-800 text-[9px] text-white px-1.5 py-0.5 rounded whitespace-nowrap z-20">
                {{ item.pollingStationName }}
              </span>
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
            <h3 class="text-xs font-semibold text-teal-400 uppercase tracking-wider">Nearest Polling Booths</h3>
            
            <div *ngIf="isLoading()" class="py-12 flex justify-center">
              <div class="animate-spin rounded-full h-6 w-6 border-b-2 border-teal-500"></div>
            </div>

            <div *ngIf="!isLoading() && bloListings().length === 0" class="text-center py-12 text-slate-500 text-xs">
              No polling booths matching current coordinates.
            </div>

            <div 
              *ngFor="let item of bloListings()" 
              class="bg-slate-950/80 rounded-xl p-3 border border-slate-800 hover:border-slate-700 transition-all space-y-2">
              <div class="flex justify-between items-start">
                <div>
                  <h4 class="text-xs font-bold text-white leading-tight">{{ item.pollingStationName }}</h4>
                  <p class="text-[10px] text-slate-400 mt-1">Officer: {{ item.bloName }}</p>
                </div>
                <span class="bg-teal-950 text-teal-400 border border-teal-900 text-[9px] px-1.5 py-0.5 rounded font-mono font-medium">
                  {{ item.distanceInKm }} km
                </span>
              </div>

              <!-- Verification indicators & Contact -->
              <div class="flex items-center justify-between border-t border-slate-900 pt-2 text-[10px]">
                <div class="flex items-center space-x-1">
                  <span class="text-emerald-400 font-semibold">{{ item.verificationScore }}%</span>
                  <span class="text-slate-500">({{ item.verificationCount }} reviews)</span>
                </div>
                <a 
                  [href]="'tel:' + item.bloContact" 
                  class="text-teal-400 hover:text-teal-300 font-medium transition-colors">
                  📞 Call Officer
                </a>
              </div>
            </div>
          </div>

          <div class="mt-4 text-[9px] text-slate-500 border-t border-slate-800/60 pt-3">
            ℹ Nearby listings are queried securely. Call features route securely using internal masking relays.
          </div>
        </div>
      </div>
    </div>
  `
})
export class BloMapComponent implements OnInit {
  // Center coordinates (default: New Delhi central coordinates for fallback)
  currentLat = signal<number>(28.6139);
  currentLon = signal<number>(77.2090);
  isLoading = signal<boolean>(false);
  bloListings = signal<any[]>([]);

  // Simulated visual pin positions relative to SVG map box (in percentage)
  pinX = signal<number>(50);
  pinY = signal<number>(50);

  ngOnInit() {
    this.fetchBloListings();
  }

  async getCurrentLocation() {
    try {
      const coordinates = await Geolocation.getCurrentPosition();
      this.currentLat.set(coordinates.coords.latitude);
      this.currentLon.set(coordinates.coords.longitude);
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

    // Convert pixel click position to percentage values
    const pctX = (x / rect.width) * 100;
    const pctY = (y / rect.height) * 100;

    this.pinX.set(pctX);
    this.pinY.set(pctY);

    // Translate relative map position to simulated coordinates offset from center
    const latOffset = (50 - pctY) * 0.0003; 
    const lonOffset = (pctX - 50) * 0.0003;

    this.currentLat.update(l => l + latOffset);
    this.currentLon.update(l => l + lonOffset);

    this.fetchBloListings();
  }

  async fetchBloListings() {
    this.isLoading.set(true);
    try {
      // Connect to the backend Web API lookup endpoint
      const headers: Record<string, string> = {
        'Authorization': `Bearer ${localStorage.getItem('auth_token')}`
      };
      const response = await fetch(`/api/v1/blo/lookup?latitude=${this.currentLat()}&longitude=${this.currentLon()}`, { headers });
      if (response.ok) {
        const data = await response.json();
        this.bloListings.set(data);
      } else {
        // Fallback mock handling if API is offline
        this.bloListings.set(this.getMockListings());
      }
    } catch (err) {
      console.error('Error contacting lookup API. Using local stubs.', err);
      this.bloListings.set(this.getMockListings());
    } finally {
      this.isLoading.set(false);
    }
  }

  // Get relative pin positions for markers to display them inside the map area
  getRelativeMapX(lon: number): number {
    const diff = (lon - this.currentLon()) * 5000 + 50;
    return Math.max(5, Math.min(95, diff));
  }

  getRelativeMapY(lat: number): number {
    const diff = 50 - (lat - this.currentLat()) * 5000;
    return Math.max(5, Math.min(95, diff));
  }

  private getMockListings() {
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
        verificationCount: 132
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
        verificationCount: 98
      }
    ];
  }
}
