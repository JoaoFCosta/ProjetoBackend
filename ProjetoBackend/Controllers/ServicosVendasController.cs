using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjetoBackend.Data;
using ProjetoBackend.Models;

namespace ProjetoBackend.Controllers
{
    [Authorize]
    public class ServicosVendasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicosVendasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ServicosVendas
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ServicoVenda.Include(s => s.Servico).Include(s => s.Venda);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ServicosVendas/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicoVenda = await _context.ServicoVenda
                .Include(s => s.Servico)
                .Include(s => s.Venda)
                .FirstOrDefaultAsync(m => m.ServicoVendaId == id);
            if (servicoVenda == null)
            {
                return NotFound();
            }

            return View(servicoVenda);
        }

        // GET: ServicosVendas/Create
        public IActionResult Create(Guid? id)
        {
            ViewData["ServicoId"] = new SelectList(_context.Servicos, "ServicoId", "Nome");
            ViewData["VendaId"] = id;
            return View();
        }

        // POST: ServicosVendas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ServicoVendaId,ServicoId,VendaId")] ServicoVenda servicoVenda)
        {
            servicoVenda.ServicoVendaId = Guid.NewGuid();
            _context.Add(servicoVenda);
            await _context.SaveChangesAsync();
            //Selecionar todos os itens dessa venda e somar o valor total, e atualizar o valor total da venda

            // lista de itens da venda
            var listaServicos = await _context.ServicoVenda.Include(s => s.Servico).Include(s => s.Venda).Where(v => v.VendaId == servicoVenda.VendaId).ToListAsync();

            // Calcular o valor total da venda
            var valorTotalVenda = listaServicos.Sum(s => s.Venda.ValorTotal);

            // Atualizar o valor total da venda correspondente
            var venda = await _context.Vendas.FindAsync(servicoVenda.VendaId);
            venda.ValorTotal = valorTotalVenda;

            // Salvar mudança no banco de dados
            await _context.SaveChangesAsync();

            ViewData["servicoId"] = servicoVenda.VendaId;

            var lista = _context.ServicoVenda.Where(s => s.VendaId == venda.VendaId).Include(v => v.Venda).ToList().OrderBy(s => s.Venda.NotaFiscal);

            if (lista.Count() == 0)
            {
                var listaVazia = new List<ServicoVenda>();
                ViewData["listaServicos"] = listaVazia;
            }
            else
            {
                ViewData["listaServicos"] = lista;
            }

            return RedirectToAction("Create", "Vendas");
        }

        // GET: ServicosVendas/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicoVenda = await _context.ServicoVenda.FindAsync(id);
            if (servicoVenda == null)
            {
                return NotFound();
            }
            ViewData["ServicoId"] = new SelectList(_context.Servicos, "ServicoId", "Nome", servicoVenda.ServicoId);
            ViewData["VendaId"] = new SelectList(_context.Vendas, "VendaId", "VendaId", servicoVenda.VendaId);
            return View(servicoVenda);
        }

        // POST: ServicosVendas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("ServicoVendaId,ServicoId,VendaId")] ServicoVenda servicoVenda)
        {
            if (id != servicoVenda.ServicoVendaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(servicoVenda);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServicoVendaExists(servicoVenda.ServicoVendaId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ServicoId"] = new SelectList(_context.Servicos, "ServicoId", "Nome", servicoVenda.ServicoId);
            ViewData["VendaId"] = new SelectList(_context.Vendas, "VendaId", "VendaId", servicoVenda.VendaId);
            return View(servicoVenda);
        }

        // GET: ServicosVendas/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicoVenda = await _context.ServicoVenda
                .Include(s => s.Servico)
                .Include(s => s.Venda)
                .FirstOrDefaultAsync(m => m.ServicoVendaId == id);
            if (servicoVenda == null)
            {
                return NotFound();
            }

            return View(servicoVenda);
        }

        // POST: ServicosVendas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var servicoVenda = await _context.ServicoVenda.FindAsync(id);
            if (servicoVenda != null)
            {
                _context.ServicoVenda.Remove(servicoVenda);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public double PrecoServico(Guid id)
        {
            var servico = _context.Servicos.Where(p => p.ServicoId == id).FirstOrDefault();
            return servico.ValorServico;
        }

        private bool ServicoVendaExists(Guid id)
        {
            return _context.ServicoVenda.Any(e => e.ServicoVendaId == id);
        }
    }
}
