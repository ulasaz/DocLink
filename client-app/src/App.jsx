import { useState, useEffect } from 'react';
import { Routes, Route, useNavigate, useParams, Link, Navigate, useLocation } from 'react-router-dom';
import './App.css';
import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import L from 'leaflet';

// --- Fix Leaflet Icons ---
import icon from 'leaflet/dist/images/marker-icon.png';
import iconShadow from 'leaflet/dist/images/marker-shadow.png';

let DefaultIcon = L.icon({
    iconUrl: icon,
    shadowUrl: iconShadow,
    iconSize: [25, 41],
    iconAnchor: [12, 41]
});
L.Marker.prototype.options.icon = DefaultIcon;

const API_BASE_URL = "http://localhost:5000";

// --- Helpers ---
function parseJwt(token) {
    try {
        if (!token) return null;
        const base64Url = token.split('.')[1];
        if (!base64Url) return null;
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join(''));
        return JSON.parse(jsonPayload);
    } catch (e) { return null; }
}

function generateMockSlots() {
    const slots = [];
    const today = new Date();
    for(let i=1; i<=3; i++) {
        const date = new Date(today);
        date.setDate(today.getDate() + i);
        slots.push({ date: date, hours: ["10:00", "11:30", "14:00", "16:30"] });
    }
    return slots;
}

const CITY_COORDINATES = {
    'Warszawa': [52.2297, 21.0122],
    'Kraków': [50.0647, 19.9450],
    'Wrocław': [51.1079, 17.0385],
    'Poznań': [52.4064, 16.9252],
    'Gdańsk': [54.3520, 18.6466],
    'Łódź': [51.7592, 19.4560],
    'Katowice': [50.2649, 19.0238],
    'Gdynia': [54.5189, 18.5305],
    'Default': [51.9194, 19.1451]
};

// ==========================================
// 1. КОМПОНЕНТЫ СТРАНИЦ
// ==========================================

const Header = ({ currentUser, onLogout }) => {
    const isDoctor = currentUser?.role === 'doctor';
    const profileLink = isDoctor ? `/doctor/${currentUser.id}` : "/my-profile";
    const profileText = isDoctor ? "My Public Page" : "My Profile";

    return (
        <header className="page-header">
            <Link to={isDoctor ? profileLink : "/search"} className="logo">DocLink</Link>
            <div className="user-nav">
                {!isDoctor && <Link to="/my-visits" className="nav-link">My Visits</Link>}
                <Link to={profileLink} className="nav-link">{profileText}</Link>
                <span style={{color:'#ddd'}}>|</span>
                <span>{currentUser?.email}</span>
                <button className="logout-btn" onClick={onLogout}>Logout</button>
            </div>
        </header>
    );
};

const WelcomePage = ({ setRole }) => (
    <div className="container" style={{textAlign:'center', marginTop:'100px'}}>
        <h1 style={{color: 'var(--primary)', fontSize: '3rem'}}>DocLink</h1>
        <div style={{display:'flex', gap:'20px', justifyContent:'center', marginTop:'40px'}}>
            <div className="doctor-card" style={{padding:'40px', textAlign:'center'}} onClick={() => setRole('patient')}>
                <h2>I am a Patient</h2>
                <p>Find a doctor and book a visit</p>
            </div>
            <div className="doctor-card" style={{padding:'40px', textAlign:'center'}} onClick={() => setRole('doctor')}>
                <h2>I am a Doctor</h2>
                <p>Manage visits and profile</p>
            </div>
        </div>
    </div>
);

const LoginPage = ({ role, onLoginSuccess }) => {
    const navigate = useNavigate();
    const [isLogin, setIsLogin] = useState(true);
    const [formData, setFormData] = useState({ email: '', password: '', firstName: '', lastName: '', specialization: '', licenseId: '' });

    const handleSubmit = async (e) => {
        e.preventDefault();
        const endpoint = isLogin ? "/api/auth/login" : "/api/auth/register";

        // --- ИЗМЕНЕНИЕ: Отправляем 'Specialist' вместо 'Doctor' ---
        const backendRole = role === 'patient' ? 'Patient' : 'Specialist';

        const payload = isLogin ? { email: formData.email, password: formData.password } : { ...formData, role: backendRole };

        try {
            const res = await fetch(`${API_BASE_URL}${endpoint}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            const data = await res.json();

            if (res.ok) {
                if (isLogin) {
                    if(!data.token) return alert("Token missing");

                    const user = parseJwt(data.token);
                    const userRealRole = user.role || user['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'patient';

                    // --- ИЗМЕНЕНИЕ: Проверка ролей с учетом Specialist ---
                    // Если в базе Specialist - это ок для роли 'doctor' на фронте
                    const normalizedDbRole = userRealRole.toLowerCase() === 'specialist' ? 'doctor' : userRealRole.toLowerCase();

                    if (normalizedDbRole !== role.toLowerCase()) {
                        alert(`Error: This account is registered as a "${userRealRole}", but you are trying to login as a "${role}".`);
                        return;
                    }

                    onLoginSuccess(data.token);
                } else {
                    alert("Registered! Please login.");
                    setIsLogin(true);
                }
            } else {
                alert(data.message || "Auth error");
            }
        } catch (e) {
            console.error(e);
            alert("Server error");
        }
    };

    return (
        <div className="container auth-container">
            <Link to="/" className="back-link">← Back</Link>
            <div className="section-card">
                <h2 style={{marginTop:0}}>{isLogin ? 'Login' : 'Register'} ({role})</h2>
                <form onSubmit={handleSubmit} className="auth-form">
                    {!isLogin && <><input className="input-field" placeholder="First Name" value={formData.firstName} onChange={e=>setFormData({...formData, firstName:e.target.value})} /><input className="input-field" placeholder="Last Name" value={formData.lastName} onChange={e=>setFormData({...formData, lastName:e.target.value})} /></>}
                    <input className="input-field" placeholder="Email" value={formData.email} onChange={e=>setFormData({...formData, email:e.target.value})} />
                    <input className="input-field" type="password" placeholder="Password" value={formData.password} onChange={e=>setFormData({...formData, password:e.target.value})} />
                    {!isLogin && role === 'doctor' && <><input className="input-field" placeholder="Spec" value={formData.specialization} onChange={e=>setFormData({...formData, specialization:e.target.value})} /><input className="input-field" placeholder="License" value={formData.licenseId} onChange={e=>setFormData({...formData, licenseId:e.target.value})} /></>}
                    <button className="action-btn" style={{marginTop:'10px'}}>{isLogin ? 'Login' : 'Register'}</button>
                </form>
                <p style={{textAlign:'center', cursor:'pointer', color:'var(--primary)', marginTop:'15px'}} onClick={() => setIsLogin(!isLogin)}>
                    {isLogin ? "No account? Sign up" : "Have an account? Login"}
                </p>
            </div>
        </div>
    );
};

const DoctorListPage = ({ currentUser, onLogout }) => {
    const navigate = useNavigate();
    const [specialists, setSpecialists] = useState([]);
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        const fetchSpecialists = async () => {
            setIsLoading(true);
            try {
                const res = await fetch(`${API_BASE_URL}/api/specialists/all`);
                if (res.ok) setSpecialists(await res.json());
            } catch (e) { console.error(e); } finally { setIsLoading(false); }
        };
        fetchSpecialists();
    }, []);

    return (
        <div className="container">
            <Header currentUser={currentUser} onLogout={onLogout} />
            <h2 style={{marginBottom:'20px'}}>Find a Specialist</h2>
            {isLoading ? <div className="loader">Loading...</div> : (
                <div className="doctor-grid">
                    {specialists.map(doc => (
                        <div key={doc.id} className="doctor-card" onClick={() => navigate(`/doctor/${doc.id}`)}>
                            <div className="doctor-card-content">
                                <div className="avatar-circle">👨‍⚕️</div>
                                <div>
                                    <h3 style={{margin:0}}>{doc.firstName} {doc.lastName}</h3>
                                    <p style={{margin:'5px 0', color:'#777'}}>{doc.specialization || "Specialist"}</p>
                                    <div className="star-rating" style={{fontSize:'0.8rem'}}>⭐⭐⭐⭐⭐</div>
                                </div>
                            </div>
                            <button className="action-btn">View Profile & Book</button>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};

const DoctorProfilePage = ({ currentUser }) => {
    const { id } = useParams();
    const navigate = useNavigate();
    const [activeDoctor, setActiveDoctor] = useState(null);
    const [selectedOfferId, setSelectedOfferId] = useState('');
    const slots = generateMockSlots();

    useEffect(() => {
        const fetchProfile = async () => {
            try {
                const res = await fetch(`${API_BASE_URL}/api/specialists/${id}`);
                if (res.ok) {
                    const data = await res.json();
                    const details = Array.isArray(data) ? data[0] : data;
                    setActiveDoctor(details);
                    if (details.offers && details.offers.length > 0) {
                        setSelectedOfferId(details.offers[0].id);
                    }
                }
            } catch (e) { console.error(e); }
        };
        fetchProfile();
    }, [id]);

    const handleBookSlot = async (dateObj, timeStr) => {
        if (currentUser.role === 'doctor') {
            alert("Doctors cannot book appointments.");
            return;
        }

        if (!selectedOfferId) return alert("Please select a service first!");
        if (!confirm(`Book appointment on ${dateObj.toLocaleDateString()} at ${timeStr}?`)) return;

        const [hours, mins] = timeStr.split(':');
        const finalDate = new Date(dateObj);
        finalDate.setHours(parseInt(hours), parseInt(mins));

        const payload = {
            patientId: currentUser.id,
            specialistId: id,
            time: finalDate.toISOString(),
            status: "Pending",
            offerId: selectedOfferId
        };

        try {
            const res = await fetch(`${API_BASE_URL}/api/appoinment/book`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${localStorage.getItem('accessToken')}` },
                body: JSON.stringify(payload)
            });
            if (res.ok) { alert("Booked!"); navigate('/my-visits'); }
            else { const err = await res.json(); alert("Error: " + err.message); }
        } catch(e) { alert("Network error"); }
    };

    if (!activeDoctor) return <div className="loader">Loading profile...</div>;

    const isOwner = currentUser?.id === id;

    return (
        <div className="container">
            <Link to={currentUser.role === 'patient' ? "/search" : "#"} className="back-link" style={isOwner ? {visibility:'hidden'} : {}}>
                ← Back to search results
            </Link>

            <div className="profile-container">
                <div className="profile-content">
                    <div className="profile-header-card">
                        <div className="profile-avatar-large">👨‍⚕️</div>
                        <div className="profile-info">
                            <h1>Dr. {activeDoctor.firstName} {activeDoctor.lastName}</h1>
                            <div className="star-rating">⭐⭐⭐⭐⭐ (5.0)</div>
                            <p className="profile-subtitle">Specialist</p>
                            {isOwner && <span style={{background:'#dbeafe', color:'#1e40af', padding:'4px 8px', borderRadius:'4px', fontSize:'0.8rem'}}>It's You</span>}
                        </div>
                    </div>

                    <div className="section-card">
                        <h3 className="section-title">Services & Offers</h3>
                        <div className="services-list">
                            {activeDoctor.offers && activeDoctor.offers.length > 0 ?
                                activeDoctor.offers.map((offer, idx) => (
                                    <span key={idx} className="service-tag">{offer.title} <span className="price-tag">{offer.price} PLN</span></span>
                                )) : <p>No specific services listed.</p>}
                        </div>
                    </div>

                    <div className="section-card">
                        <h3 className="section-title">Locations & Map</h3>
                        {activeDoctor.specialistLocations && activeDoctor.specialistLocations.length > 0 ? (
                            <div>
                                <div className="location-list">
                                    {activeDoctor.specialistLocations.map((loc, idx) => (
                                        <div key={idx} className="location-item">📍 <div><strong>{loc.city}</strong><br/>{loc.address}</div></div>
                                    ))}
                                </div>
                                <div className="map-wrapper">
                                    <MapContainer center={CITY_COORDINATES[activeDoctor.specialistLocations[0].city] || [52.2297, 21.0122]} zoom={12} style={{ height: '100%', width: '100%' }}>
                                        <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" attribution='&copy; OpenStreetMap' />
                                        {activeDoctor.specialistLocations.map((loc, idx) => {
                                            const pos = CITY_COORDINATES[loc.city];
                                            if (!pos) return null;
                                            return <Marker key={idx} position={pos}><Popup><strong>{loc.city}</strong><br/>{loc.address}</Popup></Marker>
                                        })}
                                    </MapContainer>
                                </div>
                            </div>
                        ) : <p>No locations available.</p>}
                    </div>

                    <div className="section-card">
                        <h3 className="section-title">
                            Patient Reviews
                            <span style={{fontSize:'0.9em', color:'#9ca3af', marginLeft:'10px', fontWeight:'normal'}}>
                                ({activeDoctor.reviews ? activeDoctor.reviews.length : 0})
                            </span>
                        </h3>
                        {activeDoctor.reviews && activeDoctor.reviews.length > 0 ? (
                            <div className="reviews-grid">
                                {activeDoctor.reviews.map((rev, idx) => (
                                    <div key={idx} className="review-card">
                                        <div className="review-header">
                                            <div className="reviewer-info">
                                                <div className="reviewer-avatar">{(rev.authorName || "P").charAt(0).toUpperCase()}</div>
                                                <div>
                                                    <div className="reviewer-name">{rev.authorName || "Anonymous Patient"}</div>
                                                    <div className="review-stars">{'⭐'.repeat(rev.rate)}</div>
                                                </div>
                                            </div>
                                        </div>
                                        <p className="review-content">"{rev.content}"</p>
                                    </div>
                                ))}
                            </div>
                        ) : <div style={{textAlign:'center', color:'#999', padding:'20px'}}>No reviews yet.</div>}
                    </div>
                </div>

                {!isOwner && (
                    <div className="booking-sidebar">
                        <h3 style={{marginTop:0}}>Book Appointment</h3>
                        <div style={{marginBottom: '20px'}}>
                            <label style={{fontWeight:'bold'}}>Select Service:</label>
                            <select className="booking-select" value={selectedOfferId} onChange={(e) => setSelectedOfferId(e.target.value)}>
                                {activeDoctor.offers?.map((offer, idx) => (
                                    <option key={offer.id || idx} value={offer.id}>{offer.title} - {offer.price} PLN</option>
                                ))}
                            </select>
                        </div>
                        <p style={{fontSize:'0.9rem', color:'#666'}}>Select a time slot:</p>
                        {slots.map((day, dIdx) => (
                            <div key={dIdx} style={{marginBottom:'15px'}}>
                                <div style={{fontWeight:'bold', borderBottom:'1px solid #eee'}}>{day.date.toLocaleDateString()}</div>
                                <div className="calendar-grid">
                                    {day.hours.map((time, tIdx) => (
                                        <button key={tIdx} className="slot-btn" onClick={() => handleBookSlot(day.date, time)}>{time}</button>
                                    ))}
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
};

const MyAppointmentsPage = ({ currentUser, onLogout }) => {
    const [myAppointments, setMyAppointments] = useState([]);
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        const fetchMyAppointments = async () => {
            if (!currentUser || !currentUser.id) return;
            setIsLoading(true);
            try {
                const res = await fetch(`${API_BASE_URL}/api/appoinment/patient/${currentUser.id}`, {
                    headers: { 'Authorization': `Bearer ${localStorage.getItem('accessToken')}` }
                });
                if (res.ok) setMyAppointments(await res.json());
            } catch (e) { console.error(e); } finally { setIsLoading(false); }
        };
        fetchMyAppointments();
    }, [currentUser]);

    return (
        <div className="container">
            <Header currentUser={currentUser} onLogout={onLogout} />
            <h2 style={{marginBottom: '20px'}}>My Appointments</h2>
            {isLoading ? <div className="loader">Loading...</div> : (
                <div>
                    {myAppointments.length > 0 ? myAppointments.map(app => {
                        const dateObj = new Date(app.time);
                        return (
                            <div key={app.id} className="appointment-card">
                                <div style={{display:'flex', alignItems:'center'}}>
                                    <div className="app-date-box">
                                        <span className="app-day">{dateObj.toLocaleDateString()}</span>
                                        <span className="app-time">{dateObj.toLocaleTimeString([], {hour:'2-digit', minute:'2-digit'})}</span>
                                    </div>
                                    <div className="app-details">
                                        <h3>Dr. {app.specialistName}</h3>
                                        <p className="app-address">📍 {app.city}, {app.street}</p>
                                        <div className="app-service">{app.offers && app.offers.join(", ")}</div>
                                    </div>
                                </div>
                                <span className={`status-badge status-${app.status.toLowerCase()}`}>{app.status}</span>
                            </div>
                        );
                    }) : <p>You have no appointments yet.</p>}
                </div>
            )}
        </div>
    );
};

const MyProfilePage = ({ currentUser, onLogout }) => {
    const [profile, setProfile] = useState(null);

    useEffect(() => {
        const fetchProfile = async () => {
            try {
                const res = await fetch(`${API_BASE_URL}/api/auth/patient-profile`, {
                    headers: { 'Authorization': `Bearer ${localStorage.getItem('accessToken')}` }
                });
                if (res.ok) setProfile(await res.json());
            } catch (e) { console.error(e); }
        };
        fetchProfile();
    }, []);

    return (
        <div className="container">
            <Header currentUser={currentUser} onLogout={onLogout} />
            <div className="section-card" style={{maxWidth: '600px', margin: '0 auto'}}>
                <h2 className="section-title">My Profile</h2>
                {profile ? (
                    <div style={{textAlign:'center'}}>
                        <div style={{fontSize:'60px', background:'#f3f4f6', width:'100px', height:'100px', borderRadius:'50%', margin:'0 auto 20px', display:'flex', alignItems:'center', justifyContent:'center'}}>👤</div>
                        <h3>{profile.firstName} {profile.lastName}</h3>
                        <p style={{color:'#666'}}>{profile.role || "Patient"}</p>
                        <div style={{textAlign:'left', marginTop:'20px'}}>
                            <div className="info-row"><strong>Email:</strong> <span style={{float:'right'}}>{profile.email}</span></div>
                            <div className="info-row"><strong>Phone:</strong> <span style={{float:'right'}}>{profile.phoneNumber || "-"}</span></div>
                            <div className="info-row"><strong>ID:</strong> <span style={{float:'right', fontSize:'0.8em'}}>{profile.id}</span></div>
                        </div>
                    </div>
                ) : <div className="loader">Loading...</div>}
            </div>
        </div>
    );
};

// ==========================================
// 2. ГЛАВНЫЙ КОМПОНЕНТ APP
// ==========================================

function App() {
    const navigate = useNavigate();
    const location = useLocation();

    // --- Global State ---
    const [role, setRole] = useState(null);
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [currentUser, setCurrentUser] = useState(null);

    // --- Helpers ---
    const extractUserFromToken = (token) => {
        const decoded = parseJwt(token);
        if (!decoded) return null;
        const userId = decoded.sub || decoded.id || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];

        // --- ИЗМЕНЕНИЕ: Читаем роль 'Specialist' как 'doctor' для UI ---
        const rawRole = decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'patient';
        const userRole = rawRole.toLowerCase() === 'specialist' ? 'doctor' : 'patient';

        return { id: userId, role: userRole };
    };

    // --- Effects ---
    useEffect(() => {
        const token = localStorage.getItem('accessToken');
        if (token) {
            const user = extractUserFromToken(token);
            if (user && user.id) {
                setCurrentUser(user);
                setIsLoggedIn(true);
                // Тут мы уже используем нормализованную роль (doctor/patient)
                setRole(user.role);
            } else {
                localStorage.removeItem('accessToken');
            }
        }
    }, []);

    // --- Handlers ---
    const handleLoginSuccess = (token) => {
        localStorage.setItem('accessToken', token);
        const user = extractUserFromToken(token);
        setCurrentUser(user);
        setIsLoggedIn(true);
        setRole(user.role);

        // --- РЕДИРЕКТ НА СВОЙ ПРОФИЛЬ ---
        if (user.role === 'doctor') {
            navigate(`/doctor/${user.id}`);
        } else {
            navigate('/search');
        }
    };

    const handleLogout = () => {
        setIsLoggedIn(false);
        localStorage.removeItem('accessToken');
        setCurrentUser(null);
        setRole(null);
        navigate('/');
    };

    const handleRoleSelect = (selectedRole) => {
        setRole(selectedRole);
        navigate('/login');
    };

    // --- Routing Config ---
    return (
        <Routes>
            <Route path="/" element={<WelcomePage setRole={handleRoleSelect} />} />

            <Route path="/login" element={
                <LoginPage role={role} onLoginSuccess={handleLoginSuccess} />
            } />

            {/* Protected Routes */}
            <Route path="/search" element={
                isLoggedIn ? <DoctorListPage currentUser={currentUser} onLogout={handleLogout} /> : <Navigate to="/login" />
            } />

            <Route path="/doctor/:id" element={
                isLoggedIn ? <DoctorProfilePage currentUser={currentUser} /> : <Navigate to="/login" />
            } />

            <Route path="/my-visits" element={
                isLoggedIn ? <MyAppointmentsPage currentUser={currentUser} onLogout={handleLogout} /> : <Navigate to="/login" />
            } />

            <Route path="/my-profile" element={
                isLoggedIn ? <MyProfilePage currentUser={currentUser} onLogout={handleLogout} /> : <Navigate to="/login" />
            } />

            <Route path="/dashboard" element={
                isLoggedIn && role === 'doctor' ? <div className="container"><h1>Doctor Dashboard</h1></div> : <Navigate to="/login" />
            } />
        </Routes>
    );
}

export default App;