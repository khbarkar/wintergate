const board = document.querySelector('[data-runes]');
if (board) {
  const symbols = ['ᚱ','ᚨ','ᚾ','ᛞ','ᛟ','ᛗ','ᚹ','ᛁ','ᚾ'];
  let state = [1,0,1, 0,1,0, 1,0,0];
  let moves = 0;
  let hinted = false;
  const neighbors = i => [i, i-3, i+3, i%3 ? i-1 : -1, i%3<2 ? i+1 : -1].filter(n => n >= 0 && n < 9);
  const render = () => {
    board.innerHTML = state.map((on,i) => `<button class="rune ${on?'on':''}" data-i="${i}" aria-pressed="${!!on}" aria-label="Rune ${i+1}">${symbols[i]}</button>`).join('');
    document.querySelector('[data-moves]').textContent = moves;
    const lit = state.reduce((a,b)=>a+b,0);
    document.querySelector('.meter-fill').style.width = `${lit/9*100}%`;
    document.querySelector('[data-lit]').textContent = `${lit}/9 lit`;
    if (lit === 9) {
      document.querySelector('[data-result]').innerHTML = '<span class="win">The bridge remembers the sky.</span><p>Room complete. In the full game, this unlocks the next story scene and saves progress.</p>';
      localStorage.setItem('wintergate-progress', Math.max(8, Number(localStorage.getItem('wintergate-progress')||7)));
    }
  };
  board.addEventListener('click', e => {
    const button = e.target.closest('.rune'); if (!button) return;
    const i = Number(button.dataset.i); neighbors(i).forEach(n => state[n] = state[n] ? 0 : 1); moves++; render();
  });
  document.querySelector('[data-reset]').addEventListener('click', () => { state=[1,0,1,0,1,0,1,0,0]; moves=0; render(); });
  document.querySelector('[data-hint]').addEventListener('click', () => {
    const hint = document.querySelector('.hint');
    hint.textContent = hinted ? 'A second hint: corners affect three stones; edges affect four; the centre affects five.' : 'Every press changes a cross. Work from the top row downward instead of chasing individual dark stones.';
    hinted = true;
  });
  render();
}
