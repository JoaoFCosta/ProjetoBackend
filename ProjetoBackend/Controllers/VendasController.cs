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
    public class VendasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VendasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Vendas
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Vendas.Include(v => v.Cliente);
            return View(await applicationDbContext.OrderBy(v => v.NotaFiscal).ToListAsync());
        }

        public async Task<IActionResult> Search(string nome)
        {
            if (string.IsNullOrEmpty(nome))
            {
                return RedirectToAction(nameof(Index));
            }
            var vendas = await _context.Vendas.Where(v => v.Cliente.Nome.Contains(nome)).Include(v => v.Cliente).ToListAsync();
            return View("Index", vendas.OrderBy(v => v.Cliente));
        }

        // GET: Vendas/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venda = await _context.Vendas
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(m => m.VendaId == id);
            if (venda == null)
            {
                return NotFound();
            }

            return View(venda);
        }

        // GET: Vendas/Create
        public IActionResult Create()
        {
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "Nome");
            ViewData["ProdutoId"] = new SelectList(_context.Produtos, "ProdutoId", "Nome");
            ViewData["ServicoId"] = new SelectList(_context.Servicos, "ServicoId", "Nome");
            return View();
        }

        // POST: Vendas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VendaId,NotaFiscal,ClienteId,DataVenda,ValorTotal")] Venda venda)
        {
            if (ModelState.IsValid)
            {
                // Auto incremento da nota fiscal
                venda.NotaFiscal = _context.Vendas.Count() + 1;
                venda.DataVenda = DateTime.Now;
                venda.VendaId = Guid.NewGuid();
                _context.Add(venda);
                await _context.SaveChangesAsync();
                ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "Nome", venda.ClienteId);
                ViewData["vendaId"] = venda.VendaId;
                ViewData["notaFiscal"] = venda.NotaFiscal;
                ViewData["ProdutoId"] = new SelectList(_context.Produtos, "ProdutoId", "Nome");
                ViewData["ServicoId"] = new SelectList(_context.Servicos, "ServicoId", "Nome");

                var lista = await _context.ItensVenda.Where(p => p.VendaId == venda.VendaId).Include(v => v.Venda).Include(p => p.Produto).OrderBy(p => p.Venda.NotaFiscal).ToListAsync();
                var lista2 = await _context.ServicoVenda.Where(p => p.VendaId == venda.VendaId).Include(v => v.Venda).Include(p => p.Servico).OrderBy(p => p.Venda.NotaFiscal).ToListAsync();


                if (lista.Count() == 0)
                {
                    List<ItemVenda> listaVazia = new List<ItemVenda>();
                    ViewData["listaProdutos"] = listaVazia;
                    List<ServicoVenda> listaVazia2 = new List<ServicoVenda>();
                    ViewData["listaServicos"] = listaVazia2;

                }
                else
                {
                    ViewData["listaProdutos"] = lista;
                    ViewData["listaServicos"] = lista2;
                }

                return View(venda);

            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "Nome", venda.ClienteId);
            return View(venda);
        }

        // GET: Vendas/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venda = await _context.Vendas.FindAsync(id);
            if (venda == null)
            {
                return NotFound();
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "Nome", venda.ClienteId);
            return View(venda);
        }

        // POST: Vendas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("VendaId,NotaFiscal,ClienteId,DataVenda,ValorTotal")] Venda venda)
        {
            if (id != venda.VendaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(venda);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VendaExists(venda.VendaId))
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
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "Nome", venda.ClienteId);
            return View(venda);
        }

        // GET: Vendas/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venda = await _context.Vendas
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(m => m.VendaId == id);
            if (venda == null)
            {
                return NotFound();
            }

            return View(venda);
        }

        // POST: Vendas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var venda = await _context.Vendas.FindAsync(id);
            if (venda != null)
            {
                _context.Vendas.Remove(venda);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [HttpPost, ActionName("AddProduto")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduto([Bind("ItemVendaId,VendaId,ProdutoId,Quantidade,ValorUnitario,ValorTotal")] ItemVenda? itemVenda, Guid? VendaIdS, Guid? ServicoId, double? ValorServico)
        {
            if (itemVenda.ProdutoId != null)
            {
                itemVenda.ItemVendaId = Guid.NewGuid();
                _context.Add(itemVenda);
                await _context.SaveChangesAsync();
                //Selecionar todos os itens dessa venda e somar o valor total, e atualizar o valor total da venda

                // Atualizar o valor total da venda correspondente
                var venda = await _context.Vendas.FindAsync(itemVenda.VendaId);

                // lista de itens da venda
                var listaItens = await _context.ItensVenda.Include(i => i.Produto).Include(i => i.Venda).Where(v => v.VendaId == itemVenda.VendaId).ToListAsync();

                // Calcular o valor total da venda
                var valorTotalVenda = listaItens.Sum(i => i.ValorTotal);

                // Salvar mudança no banco de dados
                await _context.SaveChangesAsync();

                ViewData["vendaId"] = itemVenda.VendaId;
                ViewData["notaFiscal"] = itemVenda.Venda.NotaFiscal;
                ViewData["ProdutoId"] = new SelectList(_context.Produtos, "ProdutoId", "Nome");
                ViewData["ServicoId"] = new SelectList(_context.Servicos, "ServicoId", "Nome");
                ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "Nome", venda.ClienteId);

                List<ItemVenda> lista = _context.ItensVenda.Where(p => p.VendaId == venda.VendaId).Include(v => v.Venda).Include(p => p.Produto).OrderBy(p => p.Venda.NotaFiscal).ToList();

                if (lista.Count() == 0)
                {
                    List<ItemVenda> listaVazia = new List<ItemVenda>();
                    ViewData["listaProdutos"] = listaVazia;
                }
                else
                {
                    ViewData["listaProdutos"] = lista;
                }

                List<ServicoVenda> lista2 = _context.ServicoVenda.Where(s => s.VendaId == venda.VendaId).Include(v => v.Venda).Include(s => s.Servico).OrderBy(s => s.Venda.NotaFiscal).ToList();

                if (lista2.Count() == 0)
                {
                    List<ServicoVenda> listaVazia = new List<ServicoVenda>();
                    ViewData["listaServicos"] = listaVazia;
                }
                else
                {
                    ViewData["listaServicos"] = lista2;
                }



                return View("Create", venda);

            }

            if (VendaIdS != null)
            {
                ServicoVenda servicoVenda = new ServicoVenda();

                servicoVenda.ServicoVendaId = Guid.NewGuid();
                servicoVenda.VendaId = VendaIdS;
                servicoVenda.ServicoId = ServicoId;

                _context.ServicoVenda.Add(servicoVenda);
                await _context.SaveChangesAsync();
                //Selecionar todos os itens dessa venda e somar o valor total, e atualizar o valor total da venda

                // Atualizar o valor total da venda correspondente
                var venda = await _context.Vendas.FindAsync(VendaIdS);

                // lista de itens da venda
                var listaItens = await _context.ServicoVenda.Include(i => i.Servico).Include(i => i.Venda).Where(v => v.VendaId == servicoVenda.VendaId).ToListAsync();



                // Salvar mudança no banco de dados
                await _context.SaveChangesAsync();

                ViewData["vendaId"] = VendaIdS;
                ViewData["notaFiscal"] = servicoVenda.Venda.NotaFiscal;
                ViewData["ProdutoId"] = new SelectList(_context.Produtos, "ProdutoId", "Nome");
                ViewData["ServicoId"] = new SelectList(_context.Servicos, "ServicoId", "Nome");
                ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "Nome", venda.ClienteId);

                List<ItemVenda> lista = _context.ItensVenda.Where(p => p.VendaId == venda.VendaId).Include(v => v.Venda).Include(p => p.Produto).OrderBy(p => p.Venda.NotaFiscal).ToList();

                if (lista.Count() == 0)
                {
                    List<ItemVenda> listaVazia = new List<ItemVenda>();
                    ViewData["listaProdutos"] = listaVazia;
                }
                else
                {
                    ViewData["listaProdutos"] = lista;
                }

                List<ServicoVenda> lista2 = _context.ServicoVenda.Where(s => s.VendaId == venda.VendaId).Include(v => v.Venda).Include(s => s.Servico).OrderBy(s => s.Venda.NotaFiscal).ToList();

                if (lista2.Count() == 0)
                {
                    List<ServicoVenda> listaVazia = new List<ServicoVenda>();
                    ViewData["listaServicos"] = listaVazia;
                }
                else
                {
                    ViewData["listaServicos"] = lista2;
                }

                return View("Create", venda);
            }

            return View("Create", itemVenda.VendaId);

        }


        [HttpPost, ActionName("FinalizarVenda")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarVenda(Guid? VendaIdTotal, double? TotalS, double? TotalP)
        {
            var venda = await _context.Vendas.Where(v => v.VendaId == VendaIdTotal).FirstAsync();
            if (venda == null)
            {
                return NotFound("Venda não encontrada");
            }

            venda.ValorTotal = TotalS + TotalP;

            _context.Vendas.Update(venda);
            await _context.SaveChangesAsync();
            var applicationDbContext = _context.Vendas.Include(v => v.Cliente);
            return View("Index", await applicationDbContext.OrderBy(v => v.NotaFiscal).ToListAsync());
        }


        private bool VendaExists(Guid id)
        {
            return _context.Vendas.Any(e => e.VendaId == id);
        }
    }
}
