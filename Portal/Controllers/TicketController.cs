using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Portal.data;
using Portal.Models.Ticket;
using Portal.Services;
using System.Text.Encodings.Web;

namespace Portal.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ISenderEmail _emailSender;
        private IWebHostEnvironment _webHostEnvironment;

        // generic methods that will help us to use UserManager and SignInManager classes

        public TicketController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ISenderEmail emailSender, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult GetData()
        {
            var tickets = _context.Tickets.ToList();
            return Json(new { data = tickets }); //this method is releated to data table

        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> DashboardAsync()
        {
            int userCount = await _userManager.Users.CountAsync();
            int OpnedTicketCount = await _context.Tickets.Where(x=>x.status=="open").CountAsync();
            int ClosedTicketCount = await _context.Tickets.Where(x => x.status == "closed").CountAsync();
            int TotalTicketCount = await _context.Tickets.CountAsync();


            ViewBag.usercount = userCount;
            ViewBag.OpnedTicketCount = OpnedTicketCount;
            ViewBag.ClosedTicketCount = ClosedTicketCount;
            ViewBag.TotalTicketCount = TotalTicketCount;
            return View();
        }



        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Handle(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == id);
            return View(ticket);
        }




        [HttpGet]


        public IActionResult LogDetails()
        {


            return View();


        }

        public IActionResult TraceMyCase()
        {


            return View();


        }


        [Authorize(Roles = "Admin")]
        public IActionResult GetLogs()
        {
            var logs = _context.Logs.ToList();
            return Json(new { data = logs }); //this method is releated to data table

        }


        public IActionResult GetMyCaseLogs()
        {
            var logs = _context.Logs.ToList().Where(x => x.AddedBy == @User?.Identity?.Name);
            return Json(new { data = logs }); //this method is releated to data table

        }




        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = _context.Tickets.FirstOrDefault(c => c.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }



        [HttpGet]
        public async Task<IActionResult> ManageAttach(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var image = _context.Uploads.ToList().Where(i => i.TicketID == id);

            if (image == null)
            {
                return NotFound();
            }

            return View(image);
        }



        [HttpGet]
        public IActionResult MyTickets()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> GetMyTicketsData()
        {
            var user = await _userManager.GetUserAsync(User);
            var mytickets = from m in _context.Tickets
                            where m.useradded == user.UserName
                            select m;

            return Json(new { data = mytickets }); //this method is releated to data table

        }




        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult OpenedCases()
        {
            return View();
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOpenedCases()
        {
            var tickets = _context.Tickets.Where(c=>c.status=="open").ToList();
            return Json(new { data = tickets });

           

        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult ClosedCases()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetClosedCases()
        {
            var tickets = _context.Tickets.Where(c => c.status == "closed").ToList();
            return Json(new { data = tickets });



        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTicketViewModel tkt)
        {
            if (ModelState.IsValid)
            {
                Random random = new Random();
                int ticketnumber = random.Next(22000000);
                _context.Add(new Ticket()

                {
                    Id = ticketnumber,
                    Title = tkt.Title,
                    Description = tkt.Description,
                    AuthorEmail = tkt.AuthorEmail,
                    AuthorMobile = tkt.AuthorMobile,
                    AuthorCompany = tkt.AuthorCompany,
                    Created = DateTime.Now,
                    status = "open",
                    useradded = @User?.Identity?.Name

                });




                _context.Add(new Log()

                {
                    Action = "Create",
                    time = DateTime.Now,
                    Message = tkt.Description,
                    TicketNumber = ticketnumber,
                    AddedBy = @User?.Identity?.Name
                });
                _context.SaveChanges();
       
                await _emailSender.SendEmailAsync(tkt.AuthorEmail, 
                    $"Case {tkt.Title}",
                    $" Dears, \r\n  Your Case with title {tkt.Title} Created Successfully we will " +
                    $"Follow Up ASAP Please follow up from Portal \r\n" +
                    $"Thanks, \r\n" +
                    $"Manages Service Team. ", true);

                var user = await _userManager.FindByNameAsync(@User?.Identity?.Name);

                bool isInRole = await _userManager.IsInRoleAsync(user, "Admin");


                TempData["create"] = "Ticket Created Successfully";
                if (isInRole)
                {
                    return RedirectToAction("Index");
                }

                else
                {
                    return RedirectToAction("MyTickets", "Ticket");
                }


            }
            return View(tkt);

        }


            





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadItem(int ticketNumber, IFormFile SelectedFile)
        {


            string root_path = _webHostEnvironment.WebRootPath;


            if (SelectedFile != null)
            {
                string filename = Guid.NewGuid().ToString();
                var upload = Path.Combine(root_path, @"Images\Uploads");
                var ext = Path.GetExtension(SelectedFile.FileName);


                using (var filestream = new FileStream(Path.Combine(upload, filename + ext), FileMode.Create))
                {
                    SelectedFile.CopyTo(filestream);
                }
                _context.Logs.Add(new Log()
                {
                    AddedBy = @User?.Identity?.Name,
                    Action = "Upload File",
                    TicketNumber = ticketNumber,
                    Message = $"New file Named {filename + ext} Uploaded To The Case",
                    time = DateTime.Now,
                }
                );

                _context.Uploads.Add(new Upload()
                {
                    UserUploaded = @User?.Identity?.Name,
                    TicketID = ticketNumber,
                    ImageURL = @"Images\Uploads\" + filename + ext,
                }
                );

                _context.SaveChanges();

            }

            TempData["create"] = "Item Uploaded sucssefully";

            return RedirectToAction("Details", "Ticket", new { id = ticketNumber });


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Handle(int id, Ticket tkt)
        {
            if (ModelState.IsValid)
            {
                var entity = _context.Tickets.Find(id);
                entity.Description = tkt.Description;
                entity.status = "closed";
                // _context.Tickets.Update(tkt);

                _context.SaveChanges();

                _context.Add(new Log()

                {
                    Action = "Close",
                    time = DateTime.Now,
                    Message = tkt.Description,
                    TicketNumber = id,
                    AddedBy = @User?.Identity?.Name

                });
                _context.SaveChanges();

                TempData["update"] = "case Updated Successfully";

            }
            return View(tkt);

        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateComment(string comment, int ticketNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(comment))
                {
                    TempData["comment"] = "comment cant be empty please enter valid update";
                    // Handle case where comment is null or empty, perhaps by returning an error response
                    return RedirectToAction("Details", "Ticket", new { id = ticketNumber, error = "Comment cannot be empty." });

                }

                _context.Add(new Log()
                {
                    Action = "Update",
                    time = DateTime.Now,
                    Message = comment,
                    TicketNumber = ticketNumber,
                    AddedBy = @User?.Identity?.Name
                });

                _context.SaveChanges();

                TempData["Update"] = "comment Updated Successfully";

                // Redirect to the details page of the updated ticket
                return RedirectToAction("Details", "Ticket", new { id = ticketNumber });
            }
            catch (Exception ex)
            {
                // Log the exception
                // You can also provide user-friendly error messages or handle the error differently

                return RedirectToAction("Details", "Ticket", new { id = ticketNumber, error = "An error occurred while updating the comment." });
            }
        }





















    }
}
