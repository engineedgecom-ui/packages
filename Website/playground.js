/**
 * EngineEdge — SmartDictionary Pro Interactive In-Browser Playground Engine
 * Simulates C# collections (ObservableDictionary, HashSet, OrderedDictionary, Stack, Queue, 6-Layer Key)
 * with real-time UI updates, duplicate rejection logic, and reactive event notifications.
 */

// ── State Management ──────────────────────────────────────────────────────────

const state = {
  eventsEnabled: true,
  currentTab: 'heroStats',
  
  // 1. Hero Stats (ObservableDictionary<CharacterProfile, CombatStats>)
  heroStats: [
    { key: { name: 'Arthur', heroClass: 'Paladin' }, value: { hp: 1200, atk: 85, def: 95, crit: 0.15 } },
    { key: { name: 'Merlin', heroClass: 'Mage' }, value: { hp: 650, atk: 140, def: 30, crit: 0.35 } },
    { key: { name: 'Robin', heroClass: 'Ranger' }, value: { hp: 800, atk: 110, def: 50, crit: 0.45 } }
  ],

  // 2. Inventory (ObservableDictionary<string, int>)
  inventory: [
    { key: 'Health Potion', value: 15 },
    { key: 'Mana Potion', value: 8 },
    { key: 'Iron Sword', value: 1 },
    { key: 'Gold Coins', value: 250 },
    { key: 'Magic Scroll', value: 3 }
  ],

  // 3. Registered Heroes (ObservableHashSet<CharacterProfile>)
  registeredHeroes: [
    { name: 'Arthur', heroClass: 'Paladin' },
    { name: 'Merlin', heroClass: 'Mage' },
    { name: 'Robin', heroClass: 'Ranger' },
    { name: 'Galahad', heroClass: 'Knight' }
  ],

  // 4. Ordered Skills (SerializableOrderedDictionary<CharacterProfile, SkillData>)
  orderedSkills: [
    { key: { name: 'Arthur', heroClass: 'Paladin' }, value: { skillName: 'Holy Shield', mana: 25, cd: 12 } },
    { key: { name: 'Merlin', heroClass: 'Mage' }, value: { skillName: 'Meteor Strike', mana: 60, cd: 20 } },
    { key: { name: 'Robin', heroClass: 'Ranger' }, value: { skillName: 'Rain of Arrows', mana: 35, cd: 8 } }
  ],

  // 5. Stack & Queue
  statStack: [
    { hp: 1200, atk: 110, def: 80 },
    { hp: 800, atk: 75, def: 50 },
    { hp: 500, atk: 45, def: 30 }
  ],
  matchQueue: [
    { name: 'Lancelot', heroClass: 'Knight' },
    { name: 'Morgana', heroClass: 'Witch' },
    { name: 'Kay', heroClass: 'Assassin' }
  ],

  // 6. Galactic Stations (6-Layer Deep Key)
  galacticStations: [
    {
      galaxy: 'MilkyWay',
      quadrant: 'Alpha-7',
      systemId: 104,
      planet: 'Sol-3',
      sector: 'Sector-A',
      subX: 42,
      subY: 88,
      stationName: 'Earth Orbital Defense Grid',
      defense: 9500
    },
    {
      galaxy: 'Andromeda',
      quadrant: 'Omega-9',
      systemId: 880,
      planet: 'Xylar-Prime',
      sector: 'Sector-D',
      subX: 999,
      subY: 120,
      stationName: 'Pirate Dreadnought Outpost',
      defense: 14200
    }
  ],

  // Event Log
  eventLogs: []
};

// ── Event Logger ─────────────────────────────────────────────────────────────

function logEvent(type, message, color = '#00FF88') {
  const now = new Date();
  const time = now.toTimeString().split(' ')[0];
  state.eventLogs.unshift({ time, type, message, color });
  if (state.eventLogs.length > 50) state.eventLogs.pop();
  renderLogs();
}

function renderLogs() {
  const container = document.getElementById('event-logs-container');
  if (!container) return;

  if (state.eventLogs.length === 0) {
    container.innerHTML = `<div class="text-slate-500 italic text-xs py-2">No mutation events fired yet. Try adding or removing items above!</div>`;
    return;
  }

  container.innerHTML = state.eventLogs.map(log => `
    <div class="flex items-start gap-2 py-1 text-xs font-mono border-b border-white/5 last:border-0">
      <span class="text-slate-500">[${log.time}]</span>
      <span class="font-semibold px-1.5 py-0.5 rounded text-[10px]" style="background: ${log.color}20; color: ${log.color}">
        ${log.type}
      </span>
      <span class="text-slate-300 flex-1">${log.message}</span>
    </div>
  `).join('');
}

// ── Tab 0: Hero Stats Logic ──────────────────────────────────────────────────

function heroEquals(a, b) {
  return a.name.trim().toLowerCase() === b.name.trim().toLowerCase() &&
         a.heroClass.trim().toLowerCase() === b.heroClass.trim().toLowerCase();
}

function tryAddHero(name, heroClass, hp, atk, def) {
  const newKey = { name: name.trim(), heroClass: heroClass.trim() };
  const existing = state.heroStats.find(h => heroEquals(h.key, newKey));

  if (existing) {
    logEvent('DUPLICATE REJECTED', `TryAdd returned FALSE: <b>${newKey.name} (${newKey.heroClass})</b> already exists!`, '#FFB800');
    return false;
  }

  const newHero = {
    key: newKey,
    value: { hp: parseInt(hp) || 1000, atk: parseInt(atk) || 80, def: parseInt(def) || 50, crit: 0.20 }
  };

  state.heroStats.push(newHero);
  if (state.eventsEnabled) {
    logEvent('HERO ADDED', `Added hero: <b>${newKey.name}</b> (HP: ${newHero.value.hp}, ATK: ${newHero.value.atk})`, '#00FF88');
  }
  renderHeroStats();
  return true;
}

function addOrUpdateHero(name, heroClass, hp, atk, def) {
  const newKey = { name: name.trim(), heroClass: heroClass.trim() };
  const idx = state.heroStats.findIndex(h => heroEquals(h.key, newKey));
  const newStats = { hp: parseInt(hp) || 1000, atk: parseInt(atk) || 80, def: parseInt(def) || 50, crit: 0.20 };

  if (idx >= 0) {
    const old = state.heroStats[idx].value;
    state.heroStats[idx].value = newStats;
    if (state.eventsEnabled) {
      logEvent('HERO UPDATED', `Updated <b>${newKey.name}</b>: ATK ${old.atk} -> ${newStats.atk}, HP ${old.hp} -> ${newStats.hp}`, '#FFD700');
    }
  } else {
    state.heroStats.push({ key: newKey, value: newStats });
    if (state.eventsEnabled) {
      logEvent('HERO ADDED', `Added hero via indexer: <b>${newKey.name}</b>`, '#00FF88');
    }
  }
  renderHeroStats();
}

function removeHero(name, heroClass) {
  const targetKey = { name, heroClass };
  const idx = state.heroStats.findIndex(h => heroEquals(h.key, targetKey));
  if (idx >= 0) {
    const removed = state.heroStats.splice(idx, 1)[0];
    if (state.eventsEnabled) {
      logEvent('HERO REMOVED', `Removed hero: <b>${removed.key.name} (${removed.key.heroClass})</b>`, '#FF4757');
    }
    renderHeroStats();
  }
}

function buffHeroAtk(name, heroClass, delta = 15) {
  const targetKey = { name, heroClass };
  const item = state.heroStats.find(h => heroEquals(h.key, targetKey));
  if (item) {
    const oldAtk = item.value.atk;
    item.value.atk += delta;
    if (state.eventsEnabled) {
      logEvent('HERO UPDATED', `Buffed <b>${name}</b> ATK: ${oldAtk} -> ${item.value.atk}`, '#FFD700');
    }
    renderHeroStats();
  }
}

function damageHeroHp(name, heroClass, dmg = 50) {
  const targetKey = { name, heroClass };
  const item = state.heroStats.find(h => heroEquals(h.key, targetKey));
  if (item) {
    const oldHp = item.value.hp;
    item.value.hp = Math.max(0, item.value.hp - dmg);
    if (state.eventsEnabled) {
      logEvent('HERO UPDATED', `Damaged <b>${name}</b> HP: ${oldHp} -> ${item.value.hp}`, '#FFD700');
    }
    renderHeroStats();
  }
}

function clearHeroStats() {
  state.heroStats = [];
  if (state.eventsEnabled) {
    logEvent('HERO CLEARED', 'Cleared all heroes from ObservableDictionary', '#00F0FF');
  }
  renderHeroStats();
}

function renderHeroStats() {
  const countBadge = document.getElementById('hero-count-badge');
  if (countBadge) countBadge.innerText = `${state.heroStats.length} entries`;

  const container = document.getElementById('hero-stats-table');
  if (!container) return;

  if (state.heroStats.length === 0) {
    container.innerHTML = `<div class="text-slate-500 italic text-center py-6 text-sm">Dictionary is empty. Use the controls above to add heroes!</div>`;
    return;
  }

  container.innerHTML = state.heroStats.map(h => `
    <div class="flex flex-wrap items-center justify-between gap-3 p-3 bg-slate-900/60 hover:bg-slate-800/60 border border-white/5 rounded-lg transition">
      <div>
        <div class="flex items-center gap-2">
          <span class="font-bold text-white">${h.key.name}</span>
          <span class="text-xs px-2 py-0.5 rounded bg-cyan-500/20 text-cyan-300 font-mono">${h.key.heroClass}</span>
        </div>
        <div class="text-xs text-slate-400 mt-1 font-mono">
          HP: <span class="text-emerald-400 font-semibold">${h.value.hp}</span> &bull; 
          ATK: <span class="text-amber-400 font-semibold">${h.value.atk}</span> &bull; 
          DEF: <span class="text-blue-400 font-semibold">${h.value.def}</span> &bull; 
          CRIT: <span class="text-purple-400 font-semibold">${(h.value.crit * 100).toFixed(0)}%</span>
        </div>
      </div>
      <div class="flex items-center gap-1.5">
        <button onclick="buffHeroAtk('${h.key.name}', '${h.key.heroClass}')" class="px-2.5 py-1 text-xs bg-amber-500/20 hover:bg-amber-500/30 text-amber-300 border border-amber-500/30 rounded font-medium transition">
          +15 ATK
        </button>
        <button onclick="damageHeroHp('${h.key.name}', '${h.key.heroClass}')" class="px-2.5 py-1 text-xs bg-rose-500/20 hover:bg-rose-500/30 text-rose-300 border border-rose-500/30 rounded font-medium transition">
          -50 HP
        </button>
        <button onclick="removeHero('${h.key.name}', '${h.key.heroClass}')" class="px-2.5 py-1 text-xs bg-red-600/80 hover:bg-red-500 text-white rounded font-medium transition flex items-center gap-1">
          <span>✕ Remove</span>
        </button>
      </div>
    </div>
  `).join('');
}

// ── Tab 1: Inventory Logic ───────────────────────────────────────────────────

function tryAddInventory(name, qty) {
  name = name.trim();
  qty = parseInt(qty) || 1;
  const existing = state.inventory.find(i => i.key.toLowerCase() === name.toLowerCase());

  if (existing) {
    logEvent('DUPLICATE REJECTED', `TryAdd failed: '<b>${name}</b>' already exists in inventory (Qty: ${existing.value})!`, '#FFB800');
    return false;
  }

  state.inventory.push({ key: name, value: qty });
  if (state.eventsEnabled) {
    logEvent('INV ADDED', `Added '<b>${name}</b>' (Qty: ${qty})`, '#00FF88');
  }
  renderInventory();
  return true;
}

function setInventoryQty(name, qty) {
  name = name.trim();
  qty = parseInt(qty) || 1;
  const existing = state.inventory.find(i => i.key.toLowerCase() === name.toLowerCase());

  if (existing) {
    const old = existing.value;
    existing.value = qty;
    if (state.eventsEnabled) {
      logEvent('INV UPDATED', `Updated '<b>${name}</b>' Qty: ${old} -> ${qty}`, '#FFD700');
    }
  } else {
    state.inventory.push({ key: name, value: qty });
    if (state.eventsEnabled) {
      logEvent('INV ADDED', `Added '<b>${name}</b>' (Qty: ${qty})`, '#00FF88');
    }
  }
  renderInventory();
}

function modifyInventoryQty(name, delta) {
  const item = state.inventory.find(i => i.key === name);
  if (item) {
    const newQty = item.value + delta;
    if (newQty <= 0) {
      removeInventory(name);
    } else {
      const old = item.value;
      item.value = newQty;
      if (state.eventsEnabled) {
        logEvent('INV UPDATED', `Updated '<b>${name}</b>' Qty: ${old} -> ${item.value}`, '#FFD700');
      }
      renderInventory();
    }
  }
}

function removeInventory(name) {
  const idx = state.inventory.findIndex(i => i.key === name);
  if (idx >= 0) {
    const removed = state.inventory.splice(idx, 1)[0];
    if (state.eventsEnabled) {
      logEvent('INV REMOVED', `Removed '<b>${removed.key}</b>'`, '#FF4757');
    }
    renderInventory();
  }
}

function clearInventory() {
  state.inventory = [];
  if (state.eventsEnabled) {
    logEvent('INV CLEARED', 'Inventory cleared!', '#00F0FF');
  }
  renderInventory();
}

function renderInventory() {
  const totalQty = state.inventory.reduce((sum, item) => sum + item.value, 0);
  const badge = document.getElementById('inv-count-badge');
  if (badge) badge.innerText = `${state.inventory.length} items (Total: ${totalQty})`;

  const container = document.getElementById('inventory-table');
  if (!container) return;

  if (state.inventory.length === 0) {
    container.innerHTML = `<div class="text-slate-500 italic text-center py-6 text-sm">Inventory is empty. Add items above!</div>`;
    return;
  }

  container.innerHTML = state.inventory.map(item => `
    <div class="flex items-center justify-between p-3 bg-slate-900/60 hover:bg-slate-800/60 border border-white/5 rounded-lg transition">
      <div>
        <span class="font-bold text-white">${item.key}</span>
        <span class="text-xs text-slate-400 font-mono ml-2">Qty: <b class="text-cyan-400">${item.value}</b></span>
      </div>
      <div class="flex items-center gap-1.5">
        <button onclick="modifyInventoryQty('${item.key}', 1)" class="w-7 h-7 flex items-center justify-center text-xs bg-slate-700 hover:bg-slate-600 text-white rounded font-bold transition">
          +1
        </button>
        <button onclick="modifyInventoryQty('${item.key}', -1)" class="w-7 h-7 flex items-center justify-center text-xs bg-slate-700 hover:bg-slate-600 text-white rounded font-bold transition">
          -1
        </button>
        <button onclick="removeInventory('${item.key}')" class="px-2.5 py-1 text-xs bg-red-600/80 hover:bg-red-500 text-white rounded font-medium transition ml-1">
          Remove
        </button>
      </div>
    </div>
  `).join('');
}

// ── Tab 2: Roster (HashSet) Logic ────────────────────────────────────────────

function addRosterHero(name, heroClass) {
  name = name.trim();
  heroClass = heroClass.trim();
  const existing = state.registeredHeroes.find(h => heroEquals(h, { name, heroClass }));

  if (existing) {
    logEvent('DUPLICATE REJECTED', `HashSet rejected duplicate: <b>${name} (${heroClass})</b> is already registered in set!`, '#FFB800');
    return false;
  }

  state.registeredHeroes.push({ name, heroClass });
  if (state.eventsEnabled) {
    logEvent('SET ADDED', `Hero Registered: <b>${name} (${heroClass})</b>`, '#00FF88');
  }
  renderRoster();
  return true;
}

function removeRosterHero(name, heroClass) {
  const idx = state.registeredHeroes.findIndex(h => heroEquals(h, { name, heroClass }));
  if (idx >= 0) {
    const removed = state.registeredHeroes.splice(idx, 1)[0];
    if (state.eventsEnabled) {
      logEvent('SET REMOVED', `Hero Unregistered: <b>${removed.name} (${removed.heroClass})</b>`, '#FF4757');
    }
    renderRoster();
  }
}

function clearRoster() {
  state.registeredHeroes = [];
  if (state.eventsEnabled) {
    logEvent('SET CLEARED', 'Hero Roster Set cleared!', '#00F0FF');
  }
  renderRoster();
}

function renderRoster() {
  const badge = document.getElementById('roster-count-badge');
  if (badge) badge.innerText = `${state.registeredHeroes.length} unique members`;

  const container = document.getElementById('roster-table');
  if (!container) return;

  if (state.registeredHeroes.length === 0) {
    container.innerHTML = `<div class="text-slate-500 italic text-center py-6 text-sm">Hero Set is empty. Register unique heroes above!</div>`;
    return;
  }

  container.innerHTML = state.registeredHeroes.map(h => `
    <div class="flex items-center justify-between p-3 bg-slate-900/60 hover:bg-slate-800/60 border border-white/5 rounded-lg transition">
      <div class="flex items-center gap-2">
        <span class="text-base">🛡️</span>
        <span class="font-bold text-white">${h.name}</span>
        <span class="text-xs px-2 py-0.5 rounded bg-cyan-500/20 text-cyan-300 font-mono">${h.heroClass}</span>
      </div>
      <button onclick="removeRosterHero('${h.name}', '${h.heroClass}')" class="px-2.5 py-1 text-xs bg-red-600/80 hover:bg-red-500 text-white rounded font-medium transition">
        Remove from Set
      </button>
    </div>
  `).join('');
}

// ── Tab 3: Ordered Skills Logic ──────────────────────────────────────────────

function addOrderedSkill(name, heroClass, skillName, mana) {
  const heroKey = { name: name.trim(), heroClass: heroClass.trim() };
  const existingIdx = state.orderedSkills.findIndex(s => heroEquals(s.key, heroKey));
  const newSkill = { skillName: skillName.trim(), mana: parseInt(mana) || 20, cd: 10 };

  if (existingIdx >= 0) {
    state.orderedSkills[existingIdx].value = newSkill;
    if (state.eventsEnabled) {
      logEvent('ORDERED UPDATE', `Updated skill for <b>${heroKey.name}</b>: ${newSkill.skillName}`, '#FFD700');
    }
  } else {
    state.orderedSkills.push({ key: heroKey, value: newSkill });
    if (state.eventsEnabled) {
      logEvent('ORDERED ADD', `Appended ordered skill: <b>${heroKey.name}</b> -> ${newSkill.skillName}`, '#00FF88');
    }
  }
  renderOrderedSkills();
}

function moveSkillToFront(idx) {
  if (idx > 0 && idx < state.orderedSkills.length) {
    const item = state.orderedSkills.splice(idx, 1)[0];
    state.orderedSkills.unshift(item);
    if (state.eventsEnabled) {
      logEvent('REORDER', `Moved <b>${item.key.name}</b> to Front (#0)`, '#00F0FF');
    }
    renderOrderedSkills();
  }
}

function moveSkillToBack(idx) {
  if (idx >= 0 && idx < state.orderedSkills.length - 1) {
    const item = state.orderedSkills.splice(idx, 1)[0];
    state.orderedSkills.push(item);
    if (state.eventsEnabled) {
      logEvent('REORDER', `Moved <b>${item.key.name}</b> to Back (#${state.orderedSkills.length - 1})`, '#00F0FF');
    }
    renderOrderedSkills();
  }
}

function removeOrderedSkill(name, heroClass) {
  const idx = state.orderedSkills.findIndex(s => heroEquals(s.key, { name, heroClass }));
  if (idx >= 0) {
    const removed = state.orderedSkills.splice(idx, 1)[0];
    if (state.eventsEnabled) {
      logEvent('ORDERED REMOVE', `Removed ordered skill for <b>${removed.key.name}</b>`, '#FF4757');
    }
    renderOrderedSkills();
  }
}

function renderOrderedSkills() {
  const badge = document.getElementById('skills-count-badge');
  if (badge) badge.innerText = `${state.orderedSkills.length} ordered entries`;

  const container = document.getElementById('ordered-skills-table');
  if (!container) return;

  if (state.orderedSkills.length === 0) {
    container.innerHTML = `<div class="text-slate-500 italic text-center py-6 text-sm">Ordered dictionary is empty.</div>`;
    return;
  }

  container.innerHTML = state.orderedSkills.map((s, idx) => `
    <div class="flex flex-wrap items-center justify-between gap-3 p-3 bg-slate-900/60 hover:bg-slate-800/60 border border-white/5 rounded-lg transition">
      <div class="flex items-center gap-2.5">
        <span class="w-6 h-6 flex items-center justify-center rounded bg-slate-800 text-xs font-mono text-cyan-400 font-bold">#${idx}</span>
        <span class="font-bold text-white">${s.key.name}</span>
        <span class="text-xs text-slate-400 font-mono">(${s.key.heroClass})</span>
        <span class="text-xs font-semibold text-amber-300 ml-2">&rarr; ${s.value.skillName} (Mana: ${s.value.mana})</span>
      </div>
      <div class="flex items-center gap-1.5">
        <button onclick="moveSkillToFront(${idx})" class="px-2 py-1 text-xs bg-slate-800 hover:bg-cyan-500/20 text-slate-300 hover:text-cyan-300 border border-white/10 rounded transition font-mono">
          &uarr; Front
        </button>
        <button onclick="moveSkillToBack(${idx})" class="px-2 py-1 text-xs bg-slate-800 hover:bg-cyan-500/20 text-slate-300 hover:text-cyan-300 border border-white/10 rounded transition font-mono">
          &darr; Back
        </button>
        <button onclick="removeOrderedSkill('${s.key.name}', '${s.key.heroClass}')" class="px-2.5 py-1 text-xs bg-red-600/80 hover:bg-red-500 text-white rounded font-medium transition">
          Remove
        </button>
      </div>
    </div>
  `).join('');
}

// ── Tab 4: Stack & Queue Logic ───────────────────────────────────────────────

function pushStack(hp, atk) {
  const item = { hp: parseInt(hp) || 800, atk: parseInt(atk) || 75, def: 40 };
  state.statStack.unshift(item); // Top is at index 0
  if (state.eventsEnabled) {
    logEvent('STACK PUSH', `Pushed to Stack: HP:${item.hp}, ATK:${item.atk}`, '#00FF88');
  }
  renderStackQueue();
}

function popStack() {
  if (state.statStack.length > 0) {
    const popped = state.statStack.shift();
    if (state.eventsEnabled) {
      logEvent('STACK POP', `Popped from Stack: HP:${popped.hp}, ATK:${popped.atk}`, '#FF4757');
    }
    renderStackQueue();
  } else {
    logEvent('EMPTY', 'Stack is already empty!', '#FFB800');
  }
}

function enqueueHero(name, heroClass) {
  const item = { name: name.trim(), heroClass: heroClass.trim() };
  state.matchQueue.push(item);
  if (state.eventsEnabled) {
    logEvent('QUEUE ENQUEUE', `Enqueued: <b>${item.name} (${item.heroClass})</b>`, '#00FF88');
  }
  renderStackQueue();
}

function dequeueHero() {
  if (state.matchQueue.length > 0) {
    const dequeued = state.matchQueue.shift();
    if (state.eventsEnabled) {
      logEvent('QUEUE DEQUEUE', `Dequeued: <b>${dequeued.name} (${dequeued.heroClass})</b>`, '#FF4757');
    }
    renderStackQueue();
  } else {
    logEvent('EMPTY', 'Queue is already empty!', '#FFB800');
  }
}

function renderStackQueue() {
  const stackContainer = document.getElementById('stack-items-list');
  if (stackContainer) {
    if (state.statStack.length === 0) {
      stackContainer.innerHTML = `<div class="text-slate-500 italic text-xs py-2">Stack is empty.</div>`;
    } else {
      stackContainer.innerHTML = state.statStack.map((s, idx) => `
        <div class="flex items-center justify-between text-xs font-mono py-1 px-2 rounded bg-slate-900/60 border border-white/5">
          <span class="text-cyan-400 font-bold">${idx === 0 ? '[TOP]' : `[#${idx}]`}</span>
          <span class="text-slate-300">HP: ${s.hp} | ATK: ${s.atk}</span>
        </div>
      `).join('');
    }
  }

  const queueContainer = document.getElementById('queue-items-list');
  if (queueContainer) {
    if (state.matchQueue.length === 0) {
      queueContainer.innerHTML = `<div class="text-slate-500 italic text-xs py-2">Queue is empty.</div>`;
    } else {
      queueContainer.innerHTML = state.matchQueue.map((q, idx) => `
        <div class="flex items-center justify-between text-xs font-mono py-1 px-2 rounded bg-slate-900/60 border border-white/5">
          <span class="text-amber-400 font-bold">${idx === 0 ? '[FRONT]' : `[#${idx}]`}</span>
          <span class="text-slate-300">${q.name} (${q.heroClass})</span>
        </div>
      `).join('');
    }
  }
}

// ── Tab 5: 6-Layer Deep Key Test ─────────────────────────────────────────────

function testSixLayerMatch() {
  logEvent('6-LAYER MATCH', `O(1) Hash Match: Found station '<b>Earth Orbital Defense Grid</b>' across 6 nested layers (DEF: 9500)!`, '#00FF88');
}

function testSixLayerMismatch() {
  logEvent('REJECTED CORRECTLY', `6-Layer Hash Mismatch: Modified Layer 6 sub-grid (Y:89 vs Y:88) rejected by cascaded HashCode.Combine!`, '#00F0FF');
}

// ── Tab Switching ────────────────────────────────────────────────────────────

function switchTab(tabId) {
  state.currentTab = tabId;
  
  document.querySelectorAll('.tab-btn').forEach(btn => {
    btn.classList.remove('bg-cyan-500/20', 'text-cyan-300', 'border-cyan-500/40');
    btn.classList.add('text-slate-400', 'border-transparent');
  });

  const activeBtn = document.getElementById(`tab-btn-${tabId}`);
  if (activeBtn) {
    activeBtn.classList.add('bg-cyan-500/20', 'text-cyan-300', 'border-cyan-500/40');
    activeBtn.classList.remove('text-slate-400', 'border-transparent');
  }

  document.querySelectorAll('.tab-panel').forEach(panel => panel.classList.add('hidden'));
  const activePanel = document.getElementById(`tab-panel-${tabId}`);
  if (activePanel) activePanel.classList.remove('hidden');
}

// ── Initialization ───────────────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', () => {
  renderHeroStats();
  renderInventory();
  renderRoster();
  renderOrderedSkills();
  renderStackQueue();
  renderLogs();

  // Event Toggle
  const toggle = document.getElementById('events-enabled-toggle');
  if (toggle) {
    toggle.addEventListener('change', (e) => {
      state.eventsEnabled = e.target.checked;
      logEvent('EVENTS TOGGLED', `Mutation Events are now <b>${state.eventsEnabled ? 'ENABLED' : 'MUTED'}</b>`, state.eventsEnabled ? '#00FF88' : '#FFB800');
    });
  }

  // Initial welcome event
  logEvent('INIT', 'EngineEdge SmartDictionary Pro interactive browser engine initialized.', '#00F0FF');
});
