const days = [
  ["Norse", "The Raven's Receipt"], ["Fairy tale", "Glass Slipper Firewall"],
  ["Animal kingdom", "The Parliament of Owls"], ["1001 Nights", "The Brass Moon"],
  ["Norse", "Bifröst Maintenance"], ["Fairy tale", "Three Bears, One Thermostat"],
  ["Animal kingdom", "Beaver Dam Bureaucracy"], ["1001 Nights", "The Djinn's Fine Print"],
  ["Norse", "Loki's Very Honest Maze"], ["Fairy tale", "Rapunzel's Escape Room"],
  ["Animal kingdom", "Octopus Switchboard"], ["1001 Nights", "The Clockwork Caravan"],
  ["Norse", "The Wolf and the Sundial"], ["Fairy tale", "Breadcrumb Protocol"],
  ["Animal kingdom", "Murmuration Station"], ["1001 Nights", "Library of Unspoken Names"],
  ["Norse", "Mjölnir Lost Property"], ["Fairy tale", "Mirror, Mirror, Offline"],
  ["Animal kingdom", "The Coral Court"], ["1001 Nights", "Forty Doors, One Key"],
  ["Norse", "Nine Realms Night Shift"], ["Fairy tale", "The Dragon's Performance Review"],
  ["Animal kingdom", "Migration Control"], ["All worlds", "The Star at World's End"]
];

function buildCalendar() {
  const root = document.querySelector('[data-calendar]');
  if (!root) return;
  const progress = Number(localStorage.getItem('wintergate-progress') || 7);
  root.innerHTML = days.map((day, index) => {
    const n = index + 1;
    const state = n <= progress ? 'done' : n === progress + 1 ? 'current' : 'locked';
    const symbol = state === 'done' ? '✓' : state === 'locked' ? '⌁' : '•';
    return `<button class="door ${state}" data-day="${n}" ${state === 'locked' ? 'disabled' : ''} aria-label="Day ${n}: ${day[1]}, ${state}">
      <span class="door-num">${String(n).padStart(2,'0')}</span><span class="door-state">${symbol}</span>
      <span class="door-theme">${day[0]}</span><span class="door-title">${day[1]}</span></button>`;
  }).join('');
  root.querySelectorAll('.door:not(.locked)').forEach(btn => btn.addEventListener('click', () => {
    if (btn.classList.contains('current')) location.href = 'puzzle.html';
    else alert(`Day ${btn.dataset.day} is complete. Chapter replay would start here.`);
  }));
}

function setupCheats() {
  const dialog = document.querySelector('#cheat-dialog');
  const open = document.querySelector('[data-open-cheat]');
  if (!dialog || !open) return;
  open.addEventListener('click', () => dialog.showModal());
  dialog.querySelector('[data-close]').addEventListener('click', () => dialog.close());
  dialog.querySelector('form').addEventListener('submit', e => {
    e.preventDefault();
    const input = dialog.querySelector('input');
    const status = dialog.querySelector('.status');
    const code = input.value.trim().toUpperCase();
    let progress = Number(localStorage.getItem('wintergate-progress') || 7);
    if (code === 'YULE-KEY') { progress = Math.min(23, progress + 1); status.textContent = 'The next door clicks open.'; }
    else if (code === 'RAVEN-24') { progress = 24; status.textContent = 'Every door is now available.'; }
    else { status.textContent = 'The lock politely pretends it did not hear that.'; return; }
    localStorage.setItem('wintergate-progress', progress);
    buildCalendar();
  });
}

buildCalendar();
setupCheats();
